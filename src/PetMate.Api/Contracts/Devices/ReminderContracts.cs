namespace PetMate.Api.Contracts.Devices;

/// <summary>Recordatorio de una próxima vacuna o desparasitación (pm-156). Solo de mascotas propias.</summary>
public sealed record ReminderResponse(Guid Id, string Kind, Guid PetId, string PetName, string Title, DateOnly DueOn, DateOnly RemindOn, bool IsRead);
