namespace PetMate.Api.Configuration;

using PetMate.Application.DependencyInjection;
using PetMate.Infrastructure.DependencyInjection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using PetMate.Api.Authentication;
using PetMate.Api.Authorization;
using PetMate.Infrastructure.Security;

/// <summary>
/// Crea el host y aplica la configuración local autorizada para T004.
/// </summary>
public static class ApplicationBootstrap
{
    /// <summary>
    /// Crea el host y, fuera de Production, incorpora el archivo <c>.env</c> ubicado
    /// exactamente en el content root. El entorno efectivo se determina primero por
    /// el host; Production nunca lee archivos locales y usa solo su configuración de host.
    /// </summary>
    public static WebApplicationBuilder CreateBuilder(
        string[] args,
        WebApplicationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(args);

        var builder = options is null
            ? WebApplication.CreateBuilder(args)
            : WebApplication.CreateBuilder(options);

        if (!builder.Environment.IsProduction())
        {
            // El content root del host limita la carga a una ubicación explícita; no se buscan padres.
            DotEnvFile.LoadIntoEnvironmentFromDirectory(builder.Environment.ContentRootPath);
            builder.Configuration.AddEnvironmentVariables();
        }

        return builder;
    }

    public static WebApplicationBuilder ConfigureCompositionRoot(WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        StartupConfigurationValidator.EnsureValid(builder.Configuration);
        builder.Services.AddApplication()
            .AddInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());
        ConfigureAuthentication(builder.Services);
        ConfigureRateLimiting(builder.Services, builder.Configuration);
        ConfigureForwardedHeaders(builder.Services, builder.Configuration, builder.Environment.IsProduction());
        // Logs estructurados (JSON) con ámbitos para la correlación (pm-054). Sin paquetes adicionales.
        builder.Logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.UseUtcTimestamp = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        });
        // El ámbito de Hosting.Diagnostics incluye RequestPath (con identificadores o texto arbitrario del cliente): se desactiva.
        // La correlación la aporta RequestLoggingMiddleware y la solicitud completa se registra con la plantilla de ruta.
        builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.None);
        // JwtBearer registra la excepción interna completa si Redis falla; el middleware global ya la registra saneada.
        builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication.JwtBearer", LogLevel.Critical);
        builder.Services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, PetMate.Api.Contracts.Responses.ApiJsonContext.Default))
            .ConfigureApiBehaviorOptions(PetMate.Api.Middleware.ApiBehaviorConfiguration.Configure);
        ConfigureCompression(builder.Services);
        builder.Services.AddOpenApi("v1", options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "PetMate API";
                document.Info.Version = "v1";
                var components = document.Components ??= new OpenApiComponents();
                components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Access Token JWT (HS256) emitido por /api/v1/auth/login, /register y /refresh. Enviar en el encabezado Authorization: Bearer.",
                };

                return Task.CompletedTask;
            });
        });
        return builder;
    }

    // Compresión de respuestas JSON (pm-048): gzip/Brotli con el nivel más rápido; también sobre HTTPS, excepto las rutas de
    // autenticación (tokens), que SensitiveAwareCompressionProvider nunca comprime. Riesgos en docs/security/compression-risk.md.
    private static void ConfigureCompression(IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.MimeTypes = ["application/json"];
        });
        services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Fastest);
        services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Fastest);
        services.AddSingleton<Microsoft.AspNetCore.ResponseCompression.IResponseCompressionProvider, PetMate.Api.Middleware.SensitiveAwareCompressionProvider>();
    }

    // Solo se confía en X-Forwarded-* de los proxies listados en TRUSTED_PROXIES (IPs separadas por coma). Sin la variable,
    // las cabeceras se ignoran y la IP es la de la conexión directa (no falsificable por el cliente). Con ForwardLimit = 1 solo debe
    // listarse el último salto (el proxy que conecta con la API).
    private static void ConfigureForwardedHeaders(IServiceCollection services, ConfigurationManager configuration, bool isProduction)
    {
        var raw = configuration["TRUSTED_PROXIES"];
        if (string.IsNullOrWhiteSpace(raw))
        {
            // H6: en Production sin TRUSTED_PROXIES la IP observada es la del proxy y el límite por IP se agrupa. Por defecto solo se
            // advierte al arrancar (el primer despliegue la deja vacía hasta medir las IPs del ingress); con REQUIRE_TRUSTED_PROXIES=true es fatal.
            if (isProduction && string.Equals(configuration["REQUIRE_TRUSTED_PROXIES"], "true", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Configuración inválida: REQUIRE_TRUSTED_PROXIES=true exige definir TRUSTED_PROXIES en Production.");
            }

            return;
        }

        var proxies = new List<System.Net.IPAddress>();
        foreach (var item in raw.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!System.Net.IPAddress.TryParse(item, out var address))
            {
                throw new InvalidOperationException("Configuración inválida: TRUSTED_PROXIES debe contener direcciones IP separadas por coma. No se muestran valores por seguridad.");
            }

            proxies.Add(address);
        }

        services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
            foreach (var proxy in proxies)
            {
                options.KnownProxies.Add(proxy);
            }
        });
    }

    private static void ConfigureRateLimiting(IServiceCollection services, ConfigurationManager configuration)
    {
        var permitLimit = ReadPerMinuteLimit(configuration, "AUTH_RATE_LIMIT_PER_MINUTE", 10);
        var petsPermitLimit = ReadPerMinuteLimit(configuration, "PETS_RATE_LIMIT_PER_MINUTE", 60);
        var usersPermitLimit = ReadPerMinuteLimit(configuration, "USERS_RATE_LIMIT_PER_MINUTE", 60);
        var carnetPermitLimit = ReadPerMinuteLimit(configuration, "CARNET_RATE_LIMIT_PER_MINUTE", 60);

        services.AddSingleton(new PetMate.Api.Authentication.AuthIpThrottleOptions(permitLimit));
        services.AddScoped<PetMate.Api.Authentication.AuthIpThrottleFilter>();
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(PetMate.Api.Controllers.AuthController.RateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                RateLimitKey.For(context.Connection.RemoteIpAddress),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }));
            // Mascotas (pm-045 CA-04): límite por usuario autenticado (claim sub), con la IP como respaldo.
            options.AddPolicy(PetMate.Api.Controllers.PetsController.RateLimitPolicy, context => PerUserWindow(context, petsPermitLimit));

            // Carnet de salud (pm-096): mismo esquema por usuario autenticado, con su propia variable.
            options.AddPolicy(PetMate.Api.Controllers.CarnetControllerBase.RateLimitPolicy, context => PerUserWindow(context, carnetPermitLimit));

            // Perfil propio (pm-036 CA-03, matriz de límites): mismo esquema por usuario autenticado.
            options.AddPolicy(PetMate.Api.Controllers.UsersController.RateLimitPolicy, context => PerUserWindow(context, usersPermitLimit));
            options.OnRejected = async (rejected, _) =>
            {
                var headers = rejected.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? new Dictionary<string, string> { ["Retry-After"] = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(System.Globalization.CultureInfo.InvariantCulture) }
                    : null;
                await PetMate.Api.Contracts.Responses.ApiResponseFactory.WriteAsync<object?>(
                    rejected.HttpContext,
                    StatusCodes.Status429TooManyRequests,
                    PetMate.Api.Contracts.Responses.ApiResponseMessages.ForStatusCode(StatusCodes.Status429TooManyRequests),
                    null,
                    headers);
            };
        });
    }

    // Ventana fija de 1 minuto por usuario autenticado (claim sub), con la IP como respaldo.
    private static RateLimitPartition<string> PerUserWindow(HttpContext context, int permitLimit)
        => RateLimitPartition.GetFixedWindowLimiter(
            context.User.FindFirst("sub")?.Value is { Length: > 0 } subject ? "u:" + subject : "ip:" + RateLimitKey.For(context.Connection.RemoteIpAddress),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true,
            });

    private static int ReadPerMinuteLimit(ConfigurationManager configuration, string name, int defaultValue)
    {
        var raw = configuration[name];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        if (!int.TryParse(raw, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var value) || value is < 1 or > 10_000)
        {
            throw new InvalidOperationException($"Configuración inválida: {name} debe ser un entero entre 1 y 10000. No se muestran valores por seguridad.");
        }

        return value;
    }

    // Autenticación JWT Bearer (T016/T017). Los parámetros salen de JwtOptions (registrado por Infrastructure) y la
    // consulta de revocación se resuelve por EventsType, sin Service Locator.
    private static void ConfigureAuthentication(IServiceCollection services)
    {
        services.AddSingleton<RevocationJwtBearerEvents>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<JwtOptions, TimeProvider>((bearer, jwt, time) =>
            {
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = JwtAccessTokenService.CreateValidationParameters(jwt, time);
                bearer.EventsType = typeof(RevocationJwtBearerEvents);
            });
        services.AddPetMateAuthorization();
    }
}
