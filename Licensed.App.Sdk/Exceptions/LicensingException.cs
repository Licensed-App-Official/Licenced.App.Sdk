using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licensed.App.Sdk.Exceptions;


[Serializable]
public class LicensingException : Exception
{
    public LicensingException() { }
    public LicensingException(string message) : base(message) { }
    public LicensingException(string message, Exception inner) : base(message, inner) { }
}
