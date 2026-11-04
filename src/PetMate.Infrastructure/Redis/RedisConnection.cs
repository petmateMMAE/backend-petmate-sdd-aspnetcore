using StackExchange.Redis;

namespace PetMate.Infrastructure.Redis;

/// <summary>
/// Conexión perezosa y reconectable a una instancia Redis. No se conecta al construirse, de modo que la API
/// arranca aunque Redis no esté disponible; <c>AbortOnConnectFail=false</c> permite reconexión automática.
/// </summary>
public abstract class RedisConnection(string connection) : IAsyncDisposable
{
    private readonly object gate = new();
    private Task<IConnectionMultiplexer>? multiplexer;

    public async Task<IDatabase> GetDatabase()
    {
        var mux = await GetMultiplexer().ConfigureAwait(false);
        return mux.GetDatabase();
    }

    public async ValueTask DisposeAsync()
    {
        Task<IConnectionMultiplexer>? current;
        lock (gate)
        {
            current = multiplexer;
            multiplexer = null;
        }

        if (current is { IsCompletedSuccessfully: true })
        {
            await current.Result.DisposeAsync().ConfigureAwait(false);
        }

        GC.SuppressFinalize(this);
    }

    private Task<IConnectionMultiplexer> GetMultiplexer()
    {
        lock (gate)
        {
            // Si el intento anterior falló, se reintenta en la siguiente llamada.
            if (multiplexer is null || multiplexer.IsFaulted || multiplexer.IsCanceled)
            {
                multiplexer = Connect();
            }

            return multiplexer;
        }
    }

    private async Task<IConnectionMultiplexer> Connect()
    {
        var options = ConfigurationOptions.Parse(connection);
        options.AbortOnConnectFail = false;
        options.ConnectTimeout = 5000;
        options.ConnectRetry = 2;
        return await ConnectionMultiplexer.ConnectAsync(options).ConfigureAwait(false);
    }
}

/// <summary>Redis exclusivo de autenticación (lista negra de JWT). Tipo distinto para evitar mezclar instancias.</summary>
public sealed class RedisAuthConnection(string connection) : RedisConnection(connection);

/// <summary>Redis exclusivo de caché distribuida. Tipo distinto para evitar mezclar instancias.</summary>
public sealed class RedisCacheConnection(string connection) : RedisConnection(connection);
