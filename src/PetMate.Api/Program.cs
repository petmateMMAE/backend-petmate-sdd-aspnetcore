using PetMate.Api.Configuration;
using PetMate.Api.Middleware;
using Scalar.AspNetCore;

var builder = ApplicationBootstrap.CreateBuilder(args);

// Falla rápido y configura el composition root sin exponer valores sensibles.
ApplicationBootstrap.ConfigureCompositionRoot(builder);

var app = builder.Build();

if (!string.IsNullOrWhiteSpace(app.Configuration["TRUSTED_PROXIES"]))
{
    app.UseForwardedHeaders();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStatusCodePages(ApiBehaviorConfiguration.WriteStatusCodeAsync);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment() && app.Services.GetRequiredService<PetMate.Infrastructure.Security.JwtOptions>().UsesPlaceholderKey)
{
    PetMate.Api.Configuration.StartupDiagnostics.PlaceholderSigningKey(app.Logger);
}

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");
    app.MapScalarApiReference("/scalar/v1", options =>
    {
        options.WithTitle("PetMate API");
        options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
    });
}

app.Run();

public partial class Program;
