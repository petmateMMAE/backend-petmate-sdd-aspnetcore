using PetMate.Application.Mediator;
using PetMate.Application.Validation;

namespace PetMate.UnitTests.Validation;

public sealed class RequestValidationPipelineTests
{
    [Fact]
    public async Task ValidRequest_ExecutesHandlerOnce()
    {
        var handler = new CountingHandler();
        var sender = CreateSender(handler, new NameValidator());

        var result = await sender.Send(new EchoCommand("Luna"));

        Assert.Equal("Luna", result);
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task InvalidRequest_ThrowsAndNeverExecutesHandler()
    {
        var handler = new CountingHandler();
        var sender = CreateSender(handler, new NameValidator());

        var exception = await Assert.ThrowsAsync<RequestValidationException>(
            () => sender.Send(new EchoCommand("")));

        Assert.Equal(0, handler.Calls);
        var failure = Assert.Single(exception.Failures);
        Assert.Equal("name", failure.Field);
        Assert.Equal("El nombre es obligatorio.", failure.Message);
    }

    [Fact]
    public async Task MultipleValidators_AccumulateAllErrors()
    {
        var handler = new CountingHandler();
        var sender = CreateSender(handler, new NameValidator(), new NameLengthValidator());

        var exception = await Assert.ThrowsAsync<RequestValidationException>(
            () => sender.Send(new EchoCommand("")));

        Assert.Equal(0, handler.Calls);
        Assert.Equal(2, exception.Failures.Count);
        Assert.Contains(exception.Failures, f => f.Message == "El nombre es obligatorio.");
        Assert.Contains(exception.Failures, f => f.Message == "El nombre debe tener al menos 2 caracteres.");
    }

    [Fact]
    public async Task RequestWithoutValidators_ExecutesHandler()
    {
        var handler = new CountingHandler();
        var sender = CreateSender(handler);

        var result = await sender.Send(new EchoCommand(""));

        Assert.Equal("", result);
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public void ValidationException_RejectsNullFailures()
        => Assert.Throws<ArgumentNullException>(() => new RequestValidationException(null!));

    private static MediatorDispatcher CreateSender(
        CountingHandler handler,
        params IRequestValidator<EchoCommand>[] validators)
        => new(new MediatorRegistry([new RequestInvoker<EchoCommand, string>(handler, validators)]));

    private sealed record EchoCommand(string Name) : ICommand<string>;

    private sealed class CountingHandler : IRequestHandler<EchoCommand, string>
    {
        public int Calls { get; private set; }

        public Task<string> Handle(EchoCommand request, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(request.Name);
        }
    }

    private sealed class NameValidator : IRequestValidator<EchoCommand>
    {
        public Task<IReadOnlyList<ValidationFailure>> Validate(EchoCommand request, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ValidationFailure>>(
                string.IsNullOrWhiteSpace(request.Name)
                    ? [new ValidationFailure("name", "El nombre es obligatorio.")]
                    : []);
    }

    private sealed class NameLengthValidator : IRequestValidator<EchoCommand>
    {
        public Task<IReadOnlyList<ValidationFailure>> Validate(EchoCommand request, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ValidationFailure>>(
                request.Name.Length < 2
                    ? [new ValidationFailure("name", "El nombre debe tener al menos 2 caracteres.")]
                    : []);
    }
}
