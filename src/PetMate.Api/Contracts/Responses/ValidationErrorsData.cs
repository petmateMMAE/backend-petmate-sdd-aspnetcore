namespace PetMate.Api.Contracts.Responses;

/// <summary>Contenido de <c>data</c> en respuestas 400 de validación.</summary>
public sealed record ValidationErrorsData(IReadOnlyList<ValidationErrorItem> Errors);

public sealed record ValidationErrorItem(string Field, string Message);
