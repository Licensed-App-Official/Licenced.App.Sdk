using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licensed.App.Sdk.Exceptions;


[Serializable]
public class FailedToDisconnectException : LicensingException
{
    public FailedToDisconnectException() { }
    public FailedToDisconnectException(string message) : base(message) { }
    public FailedToDisconnectException(string message, Exception inner) : base(message, inner) { }
}
