using PetMate.Domain.Common;

namespace PetMate.Domain.Sessions;

/// <summary>Política de vigencia de sesiones refresh: inactividad y vida absoluta (30 y 90 días por defecto).</summary>
public sealed record SessionPolicy
{
    /// <summary>Tope de seguridad para evitar desbordes y sesiones prácticamente eternas.</summary>
    public static readonly TimeSpan MaxAbsoluteLifetime = TimeSpan.FromDays(366);

    public static readonly SessionPolicy Default = new(TimeSpan.FromDays(30), TimeSpan.FromDays(90));

    public SessionPolicy(TimeSpan idleTimeout, TimeSpan absoluteLifetime)
    {
        if (idleTimeout <= TimeSpan.Zero || absoluteLifetime <= TimeSpan.Zero || idleTimeout > absoluteLifetime
            || absoluteLifetime > MaxAbsoluteLifetime)
        {
            throw new DomainException("La política de sesión requiere duraciones positivas y inactividad no mayor que la vida absoluta.");
        }

        IdleTimeout = idleTimeout;
        AbsoluteLifetime = absoluteLifetime;
    }

    public TimeSpan IdleTimeout { get; }

    public TimeSpan AbsoluteLifetime { get; }
}
