
namespace Licensed.App.Sdk.Exceptions;

/// <summary>
/// You have supplied an invalid application ID. 
/// 
/// This exception occurs when you attempt to connect using an invalid application identifier.
/// ( This stems from the StatusCode <see cref="ApiStatus.ApplicationNotFound"/>
/// </summary>
[Serializable]
public class ApplicationNotFoundException : LicensingException
{
    public ApplicationNotFoundException() { }
    public ApplicationNotFoundException(string message) : base(message) { }
    public ApplicationNotFoundException(string message, Exception inner) : base(message, inner) { }
}
