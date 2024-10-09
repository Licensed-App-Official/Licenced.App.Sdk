using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licensed.App.Sdk.Exceptions;


[Serializable]
public class LicenseInUseException : LicensingException
{
    public LicenseInUseException() { }
    public LicenseInUseException(string message) : base(message) { }
    public LicenseInUseException(string message, Exception inner) : base(message, inner) { }
}
