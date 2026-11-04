namespace PetMate.Domain.Common;

/// <summary>Violación de una invariante de dominio; el mensaje está en español y no contiene datos sensibles.</summary>
public sealed class DomainException(string message) : Exception(message);
