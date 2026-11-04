using PetMate.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Api.Controllers;
using PetMate.Application.DependencyInjection;
using PetMate.Application.Mediator;

namespace PetMate.SecurityTests.Mediator;

public sealed class MediatorDispatchTests
{
    [Fact]
    public async Task RealMvcController_SendsQueryThroughExplicitInvoker()
    {
        var services = new ServiceCollection();
        services.AddLogging().AddApplication().AddInfrastructure(PetMate.SecurityTests.Support.TestConfiguration.Valid());
        services.AddTransient<TechnicalMediatorProbeController>();

        using var provider = services.BuildServiceProvider();
        var controller = ActivatorUtilities.CreateInstance<TechnicalMediatorProbeController>(provider);

        var result = await controller.Execute();
        var response = Assert.IsType<Microsoft.AspNetCore.Mvc.ObjectResult>(result.Result);
        var body = Assert.IsType<PetMate.Api.Contracts.Responses.ApiResponse<string>>(response.Value);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(200, body.StatusCode);
        Assert.Equal("Consulta técnica procesada correctamente.", body.Message);
        Assert.Equal("mediated", body.Data);
        Assert.Equal([typeof(ISender)], typeof(TechnicalMediatorProbeController).GetConstructors()[0]
            .GetParameters().Select(parameter => parameter.ParameterType));
    }

}
