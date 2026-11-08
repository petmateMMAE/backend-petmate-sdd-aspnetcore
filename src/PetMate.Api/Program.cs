using PetMate.Api.Configuration;
using PetMate.Api.Middleware;
using Scalar.AspNetCore;

var builder = ApplicationBootstrap.CreateBuilder(args);

// Falla rápido y configura el composition root sin exponer valores sensibles.
ApplicationBootstrap.ConfigureCompositionRoot(builder);

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

if (!string.IsNullOrWhiteSpace(app.Configuration["TRUSTED_PROXIES"]))
{
    app.UseForwardedHeaders();
}

app.UseResponseCompression();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStatusCodePages(ApiBehaviorConfiguration.WriteStatusCodeAsync);
app.UseMiddleware<RequestBodyLimitMiddleware>();
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
    // Documentación pública solo en Development; con la política de respaldo debe permitirse de forma explícita.
    app.MapOpenApi("/openapi/{documentName}.json").AllowAnonymous();
    app.MapScalarApiReference("/scalar/v1", options =>
    {
        options.WithTitle("PetMate API");
        options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
    }).AllowAnonymous();
}

app.Run();

public partial class Program;
