

namespace Licensed.App.Sdk.Exceptions;

/// <summary>
/// This exception occurs when the users license has expired.
/// 
/// ( This stems from the StatusCode <see cref="ApiStatus.LicenseExpired"/> )
/// </summary>
[Serializable]
public class LicenseExpiredException : LicensingException
{
    public LicenseExpiredException() { }
    public LicenseExpiredException(string message) : base(message) { }
    public LicenseExpiredException(string message, Exception inner) : base(message, inner) { }
}
