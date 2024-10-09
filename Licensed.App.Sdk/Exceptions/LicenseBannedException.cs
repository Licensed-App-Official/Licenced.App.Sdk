
namespace Licensed.App.Sdk.Exceptions;


/// <summary>
/// This exception means the users license is banned.
/// 
/// ( This stems from the StatusCode <see cref="ApiStatus.LicenseBanned"/> )
/// </summary>
[Serializable]
public class LicenseBannedException : LicensingException
{
    public LicenseBannedException() { }
    public LicenseBannedException(string message) : base(message) { }
    public LicenseBannedException(string message, Exception inner) : base(message, inner) { }
}
