namespace PetMate.Application.Validation;

/// <summary>Error de validación de un campo; el mensaje debe estar en español.</summary>
public sealed record ValidationFailure(string Field, string Message);
