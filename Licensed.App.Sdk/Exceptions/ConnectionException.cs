
namespace Licensed.App.Sdk.Exceptions;


[Serializable]
public class ConnectionException : LicensingException
{
    public ConnectionException() { }
    public ConnectionException(string message) : base(message) { }
    public ConnectionException(string message, Exception inner) : base(message, inner) { }
}
