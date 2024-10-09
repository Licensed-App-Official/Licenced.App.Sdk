
namespace Licensed.App.Sdk.Exceptions;


/// <summary>
/// This exception indicates that an error occured on the server side.
/// 
/// ( This stems from StatusCode <see cref="ApiStatus.InternalServerError"/>. )
/// </summary>
[Serializable]
public class InternalServerErrorException : LicensingException
{
    public InternalServerErrorException() { }
    public InternalServerErrorException(string message) : base(message) { }
    public InternalServerErrorException(string message, Exception inner) : base(message, inner) { }

}
