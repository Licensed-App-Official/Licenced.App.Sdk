
namespace Licensed.App.Sdk.Exceptions;

/// <summary>
/// This exception occurs when the users license has been paused by somebody on the panel.
/// 
/// ( This stems from the StatusCode <see cref="ApiStatus.LicensePaused"/>. )
/// </summary>
[Serializable]
public class LicensePausedException : LicensingException
{
    public LicensePausedException() { }
    public LicensePausedException(string message) : base(message) { }
    public LicensePausedException(string message, Exception inner) : base(message, inner) { }
}
