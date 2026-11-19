using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PetMate.Application.Push;
using PetMate.Application.Reminders;

namespace PetMate.Infrastructure.Push;

/// <summary>
/// Trabajo periódico que envía los recordatorios vencidos (pm-155). Corre en cada instancia; la reclamación atómica de
/// <see cref="ReminderDispatcher"/> evita los envíos duplicados. Sin proveedor push configurado no hace nada. Nunca detiene la API:
/// ante un fallo registra solo el tipo de error y espera a la siguiente pasada.
/// </summary>
public sealed partial class ReminderDispatchHostedService(ReminderDispatcher dispatcher, IPushSender push, ILogger<ReminderDispatchHostedService> logger) : BackgroundService
{
    public static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!push.IsEnabled)
        {
            return;
        }

        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                var processed = await dispatcher.RunOnce(stoppingToken).ConfigureAwait(false);
                if (processed > 0)
                {
                    LogProcessed(logger, processed);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                LogFailed(logger, exception.GetType().Name);
            }
        }
        while (await WaitNext(timer, stoppingToken).ConfigureAwait(false));
    }

    private static async Task<bool> WaitNext(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    [LoggerMessage(EventId = 2204, Level = LogLevel.Information, Message = "Recordatorios procesados: {Count}.")]
    private static partial void LogProcessed(ILogger logger, int count);

    [LoggerMessage(EventId = 2205, Level = LogLevel.Warning, Message = "El trabajo de recordatorios falló ({Kind}); se reintenta en la siguiente pasada.")]
    private static partial void LogFailed(ILogger logger, string kind);
}
