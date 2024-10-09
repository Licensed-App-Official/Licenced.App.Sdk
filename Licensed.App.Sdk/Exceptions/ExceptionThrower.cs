using Newtonsoft.Json;

namespace Licensed.App.Sdk.Exceptions;

internal static class ExceptionThrower
{
    public static LicensingException GetCorrectException(string jsonData, ApiStatus status)
    {
        // Currently all errors are just a ErrorResponse.
        // As they change, we should handle each ApiStatus differently.

        var errorData = JsonConvert.DeserializeObject<ErrorResponse>(jsonData) 
            ?? throw new JsonException("The server sent incorrect JSON data.");

        return status switch
        {
            ApiStatus.Unauthenticated => new UnauthenticatedException(errorData.Message),
            ApiStatus.ApplicationNotFound => new ApplicationNotFoundException(errorData.Message),
            ApiStatus.ApplicationNotSetup => new ApplicationNotSetupException(errorData.Message),
            ApiStatus.LicenseBanned => new LicenseBannedException(errorData.Message),
            ApiStatus.LicensePaused => new LicensePausedException(errorData.Message),
            ApiStatus.LicenseExpired => new LicenseExpiredException(errorData.Message),
            ApiStatus.RateLimitExceeded => new RateLimitExceededException(errorData.Message),
            ApiStatus.InternalServerError => new InternalServerErrorException(errorData.Message),
            ApiStatus.LicenseInUse => new LicenseInUseException(errorData.Message),
            _ => throw new NotImplementedException($"BUG: Should an exception be returned for code {status}?")
        };
    }
}
