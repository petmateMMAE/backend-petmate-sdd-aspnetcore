using System.Text.Json.Serialization;

namespace PetMate.Api.Contracts.Devices;

// Contratos HTTP del registro de dispositivo (pm-151, pm-152). Se rechazan propiedades desconocidas; el usuario sale del token.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RegisterDeviceRequest(string? Token)
{
    public override string ToString() => nameof(RegisterDeviceRequest);
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record MarkReminderReadRequest(Guid? Id);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record UnregisterDeviceRequest(string? Token)
{
    public override string ToString() => nameof(UnregisterDeviceRequest);
}
