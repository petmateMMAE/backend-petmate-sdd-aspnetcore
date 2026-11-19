using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Security;

/// <summary>
/// HSTS (pm-073#CA-03): fuera de Development la API declara que solo habla por
/// HTTPS, para que el navegador no vuelva a intentar una primera petición en
/// claro. En Development no se declara, porque en local se trabaja por HTTP y la
/// marca quedaría cacheada en el navegador del equipo durante días.
///
/// Las pruebas usan el host <c>petmate.example</c> y no <c>localhost</c>:
/// ASP.NET omite la cabecera a propósito cuando el host es local, para no dejar
/// la máquina de desarrollo marcada como solo-HTTPS. Con <c>localhost</c> la
/// prueba falla aunque la configuración sea correcta.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class HstsHeaderTests
{
    [Trait("Endpoint", "health-live")]
    [Fact]
    public async Task Production_declares_hsts_for_a_year()
    {
        using var factory = new IsolatedApiFactory("Production");
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://petmate.example"),
        });

        using var response = await client.GetAsync("/api/v1/health/live");

        Assert.True(
            response.Headers.TryGetValues("Strict-Transport-Security", out var values),
            "La respuesta de producción debe traer la cabecera Strict-Transport-Security.");

        var header = string.Join(";", values!);

        // Un año. Un plazo corto reabre cada mes la ventana de la primera
        // petición en claro, que es justo lo que HSTS evita.
        Assert.Contains("max-age=31536000", header, StringComparison.Ordinal);

        // El host de Container Apps es un subdominio compartido: marcar sus
        // hermanos como solo-HTTPS afectaría a aplicaciones de terceros.
        Assert.DoesNotContain("includeSubDomains", header, StringComparison.OrdinalIgnoreCase);

        // Preload es irreversible en la práctica y no corresponde a un host prestado.
        Assert.DoesNotContain("preload", header, StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Endpoint", "health-live")]
    [Fact]
    public async Task Development_does_not_declare_hsts()
    {
        using var factory = new IsolatedApiFactory("Development");
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
        });

        using var response = await client.GetAsync("/api/v1/health/live");

        Assert.False(
            response.Headers.Contains("Strict-Transport-Security"),
            "En Development no debe declararse HSTS: dejaría el host local marcado como solo-HTTPS en el navegador.");
    }
}
