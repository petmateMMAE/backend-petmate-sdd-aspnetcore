namespace PetMate.Application.Security;

/// <summary>Resultado de verificar una contraseña contra un hash almacenado.</summary>
public enum PasswordVerificationResult
{
    /// <summary>Contraseña incorrecta o hash ilegible. No distingue ambos casos.</summary>
    Failed = 0,

    Success = 1,

    /// <summary>Contraseña correcta, pero el hash usa parámetros inferiores a los vigentes: conviene regenerarlo.</summary>
    SuccessRehashNeeded = 2,
}

/// <summary>Abstracción del hashing de contraseñas (Argon2id en Infrastructure). Nunca se almacena la contraseña en claro.</summary>
public interface IPasswordHasher
{
    /// <summary>Devuelve un hash PHC versionado con salt aleatorio. La contraseña no puede ser vacía.</summary>
    string Hash(string password);

    /// <summary>Verifica en tiempo constante. Entradas nulas, vacías o hashes mal formados devuelven <see cref="PasswordVerificationResult.Failed"/>.</summary>
    PasswordVerificationResult Verify(string password, string passwordHash);
}
