# syntax=docker/dockerfile:1
# Imagen de PetMate.Api. Multi-stage: SDK solo para compilar; runtime ligero y sin root para ejecutar.
# No se copia .env ni se incrusta ninguna credencial: todo llega por variables de entorno.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Capa de restauración cacheable: solo manifiestos y propiedades centralizadas.
COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/PetMate.Domain/PetMate.Domain.csproj src/PetMate.Domain/
COPY src/PetMate.Application/PetMate.Application.csproj src/PetMate.Application/
COPY src/PetMate.Infrastructure/PetMate.Infrastructure.csproj src/PetMate.Infrastructure/
COPY src/PetMate.Api/PetMate.Api.csproj src/PetMate.Api/
RUN dotnet restore src/PetMate.Api/PetMate.Api.csproj

COPY src/ src/
RUN dotnet publish src/PetMate.Api/PetMate.Api.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

# Almacén de CAs: el de Ubuntu 24.04 más, solo en el CD, la CA propia de los Redis de Azure (certificado público, no es
# secreto; la pasa el job deploy de .github/workflows/pipeline.yml). Se arma aquí porque la imagen final no tiene shell.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS certs
ARG REDIS_CA_CERT_B64=""
RUN if [ -n "$REDIS_CA_CERT_B64" ]; then \
      echo "$REDIS_CA_CERT_B64" | base64 -d > /usr/local/share/ca-certificates/petmate-redis-ca.crt \
      && update-ca-certificates; \
    fi

# Runtime "chiseled" (distroless de Ubuntu 24.04): sin shell, sin gestor de paquetes y sin root. Reduce la superficie
# de ataque y los paquetes que Trivy puede marcar; el escaneo bloquea cualquier HIGH/CRITICAL, tenga o no parche (pm-057).
# Globalización invariante (sin ICU ni tzdata): la API no usa culturas ni zonas horarias.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
COPY --from=certs /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/ca-certificates.crt
WORKDIR /app
COPY --from=build /app/publish .

# Usuario no root integrado en la imagen oficial (UID 1654).
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080
ENTRYPOINT ["dotnet", "PetMate.Api.dll"]
