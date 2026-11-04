using PetMate.Domain.Sessions;

namespace PetMate.Application.Authentication;

/// <summary>Refresh token recién generado: el valor en claro solo se entrega al cliente; solo el hash se persiste.</summary>
public sealed record RefreshTokenMaterial(string RawToken, RefreshTokenHash Hash)
{
    public override string ToString() => "RefreshTokenMaterial";
}

/// <summary>Genera tokens opacos criptográficamente aleatorios y calcula su hash (SHA-256).</summary>
public interface IRefreshTokenFactory
{
    RefreshTokenMaterial Create();

    /// <summary>Calcula el hash de un token recibido; <c>false</c> si no tiene el formato esperado (sin tocar el almacenamiento).</summary>
    bool TryHash(string rawToken, out RefreshTokenHash? hash);
}
