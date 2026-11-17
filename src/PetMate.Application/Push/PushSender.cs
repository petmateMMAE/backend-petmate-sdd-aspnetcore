namespace PetMate.Application.Push;

/// <summary>Contenido de un aviso push. El título y el texto nombran a la mascota y al registro (pm-155#CA-04).</summary>
public sealed record PushMessage(string Title, string Body);

/// <summary>Resultado de enviar un aviso a un dispositivo. Ningún valor es una excepción: un fallo del proveedor no rompe el flujo que originó el aviso.</summary>
public enum PushResult
{
    /// <summary>El proveedor aceptó el aviso.</summary>
    Sent = 0,

    /// <summary>El proveedor dice que el token ya no existe: quien llama debe borrarlo (pm-152#CA-02).</summary>
    InvalidToken = 1,

    /// <summary>Fallo transitorio, de configuración o de red: no se reintenta aquí y no se borra el token.</summary>
    Failed = 2,

    /// <summary>No hay proveedor configurado en este entorno.</summary>
    Disabled = 3,
}

/// <summary>
/// Puerto de envío de notificaciones push (pm-153#CA-01). Los casos de uso dependen de este puerto y no del proveedor.
/// Nunca lanza (pm-153#CA-03) y el token del dispositivo y la credencial nunca se registran.
/// </summary>
public interface IPushSender
{
    /// <summary>Falso cuando no hay proveedor configurado.</summary>
    bool IsEnabled { get; }

    Task<PushResult> Send(string deviceToken, PushMessage message, CancellationToken cancellationToken);
}
