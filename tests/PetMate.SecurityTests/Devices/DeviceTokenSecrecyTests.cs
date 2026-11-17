using PetMate.Application.Devices;

namespace PetMate.SecurityTests.Devices;

/// <summary>pm-151#CA-03: el token del dispositivo nunca aparece en texto que pueda llegar a un log.</summary>
public sealed class DeviceTokenSecrecyTests
{
    private static readonly string Secret = string.Concat("dispositivo-", new string((char)120, 24));

    [Fact]
    public void The_command_does_not_print_the_token()
        => Assert.DoesNotContain(Secret, new RegisterDeviceCommand(Guid.NewGuid(), Secret).ToString(), StringComparison.Ordinal);

    [Fact]
    public void The_http_request_does_not_print_the_token()
        => Assert.DoesNotContain(Secret, new PetMate.Api.Contracts.Devices.RegisterDeviceRequest(Secret).ToString(), StringComparison.Ordinal);

    [Fact]
    public async Task The_validator_failure_message_does_not_echo_the_token()
    {
        var failures = await new RegisterDeviceCommandValidator().Validate(new RegisterDeviceCommand(Guid.NewGuid(), "x y"), CancellationToken.None);

        Assert.Single(failures);
        Assert.DoesNotContain("x y", failures[0].Message, StringComparison.Ordinal);
    }
}
