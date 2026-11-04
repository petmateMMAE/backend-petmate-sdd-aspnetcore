using System.Security.Cryptography;
using System.Text;
using Geralt;
using PetMate.Application.Security;

namespace PetMate.Infrastructure.Security;

/// <summary>
/// Argon2id mediante libsodium (Geralt): salt aleatorio por hash, cadena PHC versionada, verificación en
/// tiempo constante y detección de parámetros obsoletos. No registra ni conserva contraseñas ni hashes.
/// </summary>
public sealed partial class Argon2idPasswordHasher(Argon2idOptions options) : IPasswordHasher, IDisposable
{
    // Cabecera PHC esperada. Los parámetros del hash almacenado se acotan antes de verificar para que un
    // hash manipulado (m o t enormes) no pueda agotar memoria o CPU.
    [System.Text.RegularExpressions.GeneratedRegex(@"^\$argon2id\$v=19\$m=(\d{1,7}),t=(\d{1,3}),p=1\$")]
    private static partial System.Text.RegularExpressions.Regex PhcHeader();

    private const string PhcPrefix = "$argon2id$";
    private const int MaxPasswordBytes = 1024;
    private const int MaxHashChars = 255;

    private static readonly TimeSpan GateTimeout = TimeSpan.FromSeconds(5);

    // Acota los hashes simultáneos (cada uno usa decenas de MiB): ante saturación se responde 503 en vez de agotar memoria.
    private readonly SemaphoreSlim gate = new(options.MaxConcurrency, options.MaxConcurrency);

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        try
        {
            if (passwordBytes.Length > MaxPasswordBytes)
            {
                throw new ArgumentException("La contraseña excede el tamaño máximo permitido.", nameof(password));
            }

            Acquire();
            try
            {
                Span<char> hash = stackalloc char[Argon2id.HashSize];
                Argon2id.ComputeHash(hash, passwordBytes, options.Iterations, MemoryBytes(options.MemoryKiB));
                var end = hash.IndexOf('\0');
                return new string(end < 0 ? hash : hash[..end]);
            }
            finally
            {
                gate.Release();
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }

    public PasswordVerificationResult Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash)
            || passwordHash.Length > MaxHashChars || !passwordHash.StartsWith(PhcPrefix, StringComparison.Ordinal)
            || !HasSafeParameters(passwordHash))
        {
            return PasswordVerificationResult.Failed;
        }

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        try
        {
            if (passwordBytes.Length > MaxPasswordBytes)
            {
                return PasswordVerificationResult.Failed;
            }

            // libsodium espera la cadena terminada en NUL dentro de un búfer de tamaño fijo.
            Span<char> hash = stackalloc char[Argon2id.HashSize];
            hash.Clear();
            if (passwordHash.Length >= hash.Length)
            {
                return PasswordVerificationResult.Failed;
            }

            passwordHash.AsSpan().CopyTo(hash);

            Acquire();
            try
            {
                if (!Argon2id.VerifyHash(hash, passwordBytes))
                {
                    return PasswordVerificationResult.Failed;
                }

                return Argon2id.NeedsRehash(hash, options.Iterations, MemoryBytes(options.MemoryKiB))
                    ? PasswordVerificationResult.SuccessRehashNeeded
                    : PasswordVerificationResult.Success;
            }
            finally
            {
                gate.Release();
            }
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or CryptographicException)
        {
            // Hash mal formado o no verificable: se trata como fallo, sin detallar el motivo.
            return PasswordVerificationResult.Failed;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }

    public void Dispose() => gate.Dispose();

    private void Acquire()
    {
        if (!gate.Wait(GateTimeout))
        {
            throw new AuthDependencyUnavailableException();
        }
    }

    private static bool HasSafeParameters(string passwordHash)
    {
        var match = PhcHeader().Match(passwordHash);
        return match.Success
            && int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) is >= Argon2idOptions.MinMemoryKiB and <= Argon2idOptions.MaxMemoryKiB
            && int.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture) is >= Argon2idOptions.MinIterations and <= Argon2idOptions.MaxIterations;
    }

    private static int MemoryBytes(int memoryKiB) => checked(memoryKiB * 1024);
}
