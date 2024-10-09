

namespace Licensed.App.Sdk.Exceptions;

/// <summary>
/// The clients session has expired. Please note, this will not happen if the heartbeat thread is running.
/// The heartbeat allows the server to keep a close eye on the client, so we keep extending the expiration.
/// If this is thrown, it's likely because the heartbeat has been stopped.
/// 
/// ( caused when status <see cref="ApiStatus.SessionExpired"/> is returned. )
/// </summary>
[Serializable]
public class SessionExpiredException : LicensingException
{
    public SessionExpiredException() { }
    public SessionExpiredException(string message) : base(message) { }
    public SessionExpiredException(string message, Exception inner) : base(message, inner) { }
}
