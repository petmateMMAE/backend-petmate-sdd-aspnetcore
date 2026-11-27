using System.Reflection;
using PetMate.Application.Security;

namespace PetMate.UnitTests.Authentication;

/// <summary>
/// Todo ámbito de <see cref="AuthThrottleRules"/> tiene que caber en la clave de Redis.
///
/// Existe por un fallo real en producción: el límite era de 16 caracteres y "public-business-ip" mide 18, así
/// que CADA llamada al mapa público —la pantalla principal de la app— lanzaba ArgumentException y devolvía
/// HTTP 500. "deletion-request-email" (22) habría hecho lo mismo con el borrado de cuenta en cuanto se usara.
///
/// La prueba recorre las constantes por reflexión, no una lista escrita a mano: un ámbito nuevo que no quepa
/// falla aquí, en lugar de fallar en producción.
/// </summary>
public sealed class ThrottleScopeLengthTests
{
    // Mismo valor que RedisAuthAttemptThrottle.MaxScopeLength, que es interno a Infrastructure.
    private const int MaxScopeLength = 32;

    public static TheoryData<string, string> Scopes()
    {
        var data = new TheoryData<string, string>();
        foreach (var field in typeof(AuthThrottleRules).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field is { IsLiteral: true, FieldType: var type } && type == typeof(string) && field.Name.EndsWith("Scope", StringComparison.Ordinal))
            {
                data.Add(field.Name, (string)field.GetRawConstantValue()!);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Scopes))]
    public void Every_scope_fits_the_redis_key_limit(string name, string scope)
    {
        Assert.False(string.IsNullOrWhiteSpace(scope), $"{name} está vacío.");
        Assert.True(scope.Length <= MaxScopeLength, $"{name} = \"{scope}\" mide {scope.Length}; el máximo es {MaxScopeLength}.");
        Assert.True(scope.All(c => char.IsAsciiLetterLower(c) || c == '-'), $"{name} = \"{scope}\" solo admite minúsculas ASCII y guiones.");
    }

    [Fact]
    public void There_is_at_least_one_scope_to_check()
    {
        // Si alguien renombra las constantes, la teoría de arriba se quedaría sin casos y pasaría en vacío.
        Assert.NotEmpty(Scopes());
    }
}
