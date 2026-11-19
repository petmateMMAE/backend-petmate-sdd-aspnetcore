using System.Text.Json.Serialization;

namespace PetMate.Api.Contracts.Devices;

// Contratos de las preferencias de aviso (pm-157). Reemplazo completo; se rechazan miembros desconocidos.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record UpdateNotificationSettingsRequest(bool? Vaccines, bool? Reviews, bool? Chat, bool? Followups, string? Sound, int? ReminderDaysBefore);

public sealed record NotificationSettingsResponse(bool Vaccines, bool Reviews, bool Chat, bool Followups, string Sound, int ReminderDaysBefore);
