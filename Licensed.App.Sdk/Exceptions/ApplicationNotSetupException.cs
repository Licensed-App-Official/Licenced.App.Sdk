
namespace Licensed.App.Sdk.Exceptions;

/// <summary>
/// This exception is thrown when your application has not been setup correctly. We require that your
/// application contains a salt. One is automatically generated when you create your application,
/// but it appears it's been removed.
/// 
/// This salt is used for bcrypt when hashing the session identifier.
/// 
/// ( This stems from the StatusCode <see cref="ApiStatus.ApplicationNotSetup"/>. )
/// </summary>
[Serializable]
public class ApplicationNotSetupException : LicensingException
{
    public ApplicationNotSetupException() { }
    public ApplicationNotSetupException(string message) : base(message) { }
    public ApplicationNotSetupException(string message, Exception inner) : base(message, inner) { }
}
