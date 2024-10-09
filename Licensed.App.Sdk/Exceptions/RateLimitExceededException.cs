using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licensed.App.Sdk.Exceptions;

/// <summary>
/// This exception occurs when the API is ratelimiting this client.
/// 
/// This is a very unlikely scenario and if it occurs, you are likely sending too many requests.
/// 
/// ( This stems from StatusCode <see cref="ApiStatus.RateLimitExceeded"/>. )
/// </summary>
[Serializable]
public class RateLimitExceededException : LicensingException
{
    public RateLimitExceededException() { }
    public RateLimitExceededException(string message) : base(message) { }
    public RateLimitExceededException(string message, Exception inner) : base(message, inner) { }
}
