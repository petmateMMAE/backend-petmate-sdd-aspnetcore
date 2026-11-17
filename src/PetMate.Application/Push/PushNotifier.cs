using PetMate.Application.Devices;
using PetMate.Application.Notifications;
using PetMate.Application.Persistence;

namespace PetMate.Application.Push;

/// <summary>Tema de un aviso; decide qué preferencia del usuario (pm-157) lo gobierna.</summary>
public enum NotificationTopic
{
    /// <summary>Mensajes del chat.</summary>
    Chat = 1,

    /// <summary>Cambios del seguimiento (estado o próxima revisión).</summary>
    Followups = 2,
}

/// <summary>
/// Avisa por push a una cuenta respetando sus preferencias (pm-158, pm-159). Envía a todos sus dispositivos y borra los tokens que el
/// proveedor reporta inválidos (pm-152#CA-02). Nunca lanza: si el proveedor o la base fallan, el aviso se pierde y la operación que
/// lo originó sigue (pm-153#CA-03).
/// </summary>
// shortcut: sin cola ni reintentos; un aviso que falla se pierde. Subir a una cola si se necesita entrega garantizada.
public sealed class PushNotifier(IPushSender push, IDeviceRepository devices, INotificationSettingsRepository settings)
{
    public async Task Notify(Guid userId, NotificationTopic topic, PushMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!push.IsEnabled)
        {
            return;
        }

        try
        {
            var preferences = await settings.Find(userId, cancellationToken).ConfigureAwait(false) ?? NotificationSettings.Default(userId);
            if (!(topic == NotificationTopic.Chat ? preferences.Chat : preferences.Followups))
            {
                return;
            }

            foreach (var token in await devices.ListTokens(userId, cancellationToken).ConfigureAwait(false))
            {
                if (await push.Send(token, message, cancellationToken).ConfigureAwait(false) == PushResult.InvalidToken)
                {
                    await devices.Unregister(userId, token, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (PersistenceUnavailableException)
        {
            // Sin base de datos no hay aviso; la operación original ya se completó.
        }
    }
}
