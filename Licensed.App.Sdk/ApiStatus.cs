using System.Net;

namespace Licensed.App.Sdk;

/// <summary>
/// Represents the various API status codes used in the Licensed App SDK.
/// </summary>
public enum ApiStatus : int
{
    /// <summary>
    /// The request was successful. (HTTP 200)
    /// </summary>
    Ok = 200,

    /// <summary>
    /// The client has been successfully disconnected. (HTTP 201)
    /// </summary>
    Disconnected = 201,

    /// <summary>
    /// The request lacks valid authentication credentials. (HTTP 401)
    /// </summary>
    Unauthenticated = 401,

    /// <summary>
    /// The requested application was not found. (HTTP 404)
    /// </summary>
    ApplicationNotFound = 404,

    /// <summary>
    /// The application is not properly set up. (HTTP 406)
    /// </summary>
    ApplicationNotSetup = 406,

    /// <summary>
    /// The license has been banned. (HTTP 403)
    /// </summary>
    LicenseBanned = 403,

    /// <summary>
    /// The license has been paused. (HTTP 407)
    /// </summary>
    LicensePaused = 407,

    /// <summary>
    /// The license has expired. (HTTP 408)
    /// </summary>
    LicenseExpired = 408,

    /// <summary>
    /// The license is being used on another client. (HTTP 409)
    /// </summary>
    LicenseInUse = 409,

    /// <summary>
    /// The rate limit for API requests has been exceeded. (HTTP 429)
    /// </summary>
    RateLimitExceeded = 429,

    /// <summary>
    /// The session has expired. (HTTP 410)
    /// </summary>
    SessionExpired = 410,

    /// <summary>
    /// The requested resource was not found. (HTTP 411)
    /// </summary>
    NotFound = 411,

    /// <summary>
    /// An internal server error occurred. (HTTP 500)
    /// </summary>
    InternalServerError = 500,

    /// <summary>
    /// The status code is unknown or not specifically handled. (Custom 1000)
    /// </summary>
    Unknown = 1000
}

/// <summary>
/// Provides extension methods for the ApiStatus enum.
/// </summary>
public static class ApiStatusExtensions
{
    /// <summary>
    /// Converts an HTTP status code to the corresponding ApiStatus.
    /// </summary>
    /// <param name="_">This parameter is not used and can be ignored. It's present to make this an extension method.</param>
    /// <param name="statusCode">The HTTP status code to convert.</param>
    /// <returns>The corresponding ApiStatus, or ApiStatus.Unknown if the status code is not recognized.</returns>
    public static ApiStatus FromStatusCode(this ApiStatus _, HttpStatusCode statusCode)
    {
        return Enum.IsDefined(typeof(ApiStatus), (int)statusCode)
            ? (ApiStatus)(int)statusCode
            : ApiStatus.Unknown;
    }
}