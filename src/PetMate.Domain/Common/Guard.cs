namespace PetMate.Domain.Common;

internal static class Guard
{
    public static DateTime Utc(DateTime value, string name)
        => value.Kind == DateTimeKind.Utc
            ? value
            : throw new DomainException($"La fecha '{name}' debe estar expresada en UTC.");

    public static Guid NotEmpty(Guid value, string name)
        => value != Guid.Empty ? value : throw new DomainException($"El identificador '{name}' no puede estar vacío.");
}
