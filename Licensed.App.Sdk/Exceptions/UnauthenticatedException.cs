
namespace Licensed.App.Sdk.Exceptions;

/// <summary>
/// This exception describes a scenario where the user has entered an invalid license.
/// 
/// I.E The API call to /connect was supplied an invalid license key, and the API returned
/// status code <see cref="ApiStatus.Unauthenticated"/>.
/// </summary>

[Serializable]
public class UnauthenticatedException : LicensingException
{
    public UnauthenticatedException() { }
    public UnauthenticatedException(string message) : base(message) { }
    public UnauthenticatedException(string message, Exception inner) : base(message, inner) { }
}
