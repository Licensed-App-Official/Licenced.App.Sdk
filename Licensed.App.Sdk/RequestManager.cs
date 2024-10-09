using Licensed.App.Sdk.Exceptions;
using Licensed.App.Sdk.Types;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Web;

namespace Licensed.App.Sdk;

/// <summary>
/// Delegate for handling exceptions that occur during API requests.
/// </summary>
/// <param name="exception">The exception that occurred.</param>
/// <param name="statusCode">The API status code associated with the exception.</param>
public delegate void RequestExceptionHandler(Exception exception, ApiStatus statusCode);

/// <summary>
/// Delegate for handling cases where rate limits are being imposed.
/// </summary>
/// <param name="rateLimits">The current limits, that are being exceeded.</param>
public delegate void RatelimitedHandler(RateLimits rateLimits);

/// <summary>
/// Manages API requests for the Licensed App SDK.
/// </summary>
public class RequestManager
{
    /// <summary>
    /// The Licensed App instance associated with this RequestManager.
    /// </summary>
    internal readonly LicensedApp App;

    /// <summary>
    /// Handler for exceptions that occur during API requests.
    /// </summary>
    internal RequestExceptionHandler? ExceptionHandler;

    /// <summary>
    /// Initializes a new instance of the RequestManager class.
    /// </summary>
    /// <param name="app">The Licensed App instance to associate with this RequestManager.</param>
    public RequestManager(LicensedApp app)
    {
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls13;
        App = app;
    }

    /// <summary>
    /// Gets the base request string for API calls.
    /// </summary>
    /// <returns>The base URL for API requests.</returns>
    public string GetBaseRequestString() => $"{App.Metadata.Base}";

    /// <summary>
    /// Creates and configures an HttpClient for API requests.
    /// </summary>
    /// <returns>A configured HttpClient instance.</returns>
    private HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(App.Metadata.BaseUrl)
        };
        if (App.EnableDebugLogging)
            Console.Error.WriteLine($"Created HttpClient with base address: {App.Metadata.BaseUrl}");
        return httpClient;
    }

    /// <summary>
    /// Handles the rate limit headers from the API response.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    private void HandleRateLimitHeaders(HttpResponseMessage response)
    {
        int limit = 0;
        int remaining = 0;
        DateTime reset = DateTime.MinValue;

        if (response.Headers.TryGetValues("X-RateLimit-Limit", out var limitValues))
        {
            if (int.TryParse(limitValues.FirstOrDefault(), out int parsedLimit))
            {
                limit = parsedLimit;
            }
        }

        if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remainingValues))
        {
            if (int.TryParse(remainingValues.FirstOrDefault(), out int parsedRemaining))
            {
                remaining = parsedRemaining;
            }
        }

        if (response.Headers.TryGetValues("X-RateLimit-Reset", out var resetValues))
        {
            if (long.TryParse(resetValues.FirstOrDefault(), out long unixTimestamp))
            {
                reset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime.ToLocalTime();
            }
        }

        App.Metadata.RateLimits.Fill(limit, remaining, reset);

        if (App.EnableDebugLogging)
            Console.Error.WriteLine($"Rate Limit: {limit}, Remaining: {remaining}, Reset: {reset}");
    }

    public async Task<bool> SendHeartbeat()
    {
        Debug.Assert(App.SessionId != null, "Cannot send heartbeat without being connected.");

        using var httpClient = CreateHttpClient();

        var content = new StringContent(JsonConvert.SerializeObject(new { sessionId = App.SessionId, applicationId = App.Metadata.ApplicationId }), System.Text.Encoding.UTF8, "application/json");

        if (App.EnableDebugLogging)
            Console.Error.WriteLine($"Sending POST request to: /api/v1/heartbeat with content: {await content.ReadAsStringAsync()}");

        var response = await httpClient.PostAsync("/api/v1/heartbeat", content);
        var result = await HandleResponse<HeartbeatResponse>(response, ApiStatus.Ok);

        return result?.Success ?? false;
    }

    /// <summary>
    /// Handles the API response and potential exceptions.
    /// </summary>
    /// <typeparam name="T">The type of the expected response object.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="expectedStatus">The expected API status for a successful response.</param>
    /// <returns>The deserialized response object if successful, null otherwise.</returns>
    private async Task<T?> HandleResponse<T>(HttpResponseMessage response, ApiStatus expectedStatus) where T : class
    {
        HandleRateLimitHeaders(response);

        var status = ApiStatus.Unknown.FromStatusCode(response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();

        if (App.EnableDebugLogging)
            Console.Error.WriteLine($"Received response with status: {status}, JSON: {json}");

        if (status == ApiStatus.RateLimitExceeded)
        {
            if (App.EnableDebugLogging)
                Console.Error.WriteLine("Rate limit exceeded. Please try again later.");
            return null;
        }

        if (status == ApiStatus.NotFound)
        {
            if (App.EnableDebugLogging)
                Console.Error.WriteLine("Resource not found.");
            return null;
        }

        if (status != expectedStatus)
        {
            var exception = ExceptionThrower.GetCorrectException(json, status);
            if (ExceptionHandler != null)
            {
                if (App.EnableDebugLogging)
                    Console.Error.WriteLine($"Exception occurred: {exception.Message}");
                ExceptionHandler.Invoke(exception, status);
                return null;
            }
            else
                throw exception;
        }

        try
        {
            var result = JsonConvert.DeserializeObject<T>(json);
            if (App.EnableDebugLogging)
                Console.Error.WriteLine($"Deserialized response: {result}");
            return result;
        }
        catch (JsonException ex)
        {
            if (App.EnableDebugLogging)
                Console.Error.WriteLine($"JSON deserialization error: {ex.Message}");
            throw new JsonException("The server responded with invalid JSON.", ex);
        }
    }

    /// <summary>
    /// Retrieves a variable from the API.
    /// </summary>
    /// <param name="variableName">The name of the variable to retrieve.</param>
    /// <returns>A Variable object if found, null otherwise.</returns>
    public async Task<Variable?> GetVariable(string variableName)
    {
        Debug.Assert(App.SessionId != null, "You are attempting to call GetVariable() without being connected.");

        using var httpClient = CreateHttpClient();

        var encodedSessionId = HttpUtility.UrlEncode(App.SessionId);
        var encodedApplicationId = HttpUtility.UrlEncode(App.Metadata.ApplicationId);
        var encodedVariableName = HttpUtility.UrlEncode(variableName);

        var getUrl = $"api/v1/variable?sessionId={encodedSessionId}&applicationId={encodedApplicationId}&name={encodedVariableName}";

        if (App.EnableDebugLogging)
            Console.Error.WriteLine($"Sending GET request to: {getUrl}");

        var response = await httpClient.GetAsync(getUrl);
        return await HandleResponse<Variable>(response, ApiStatus.Ok);
    }

    /// <summary>
    /// Disconnects the current session.
    /// </summary>
    /// <returns>A DisconnectResponse object if successful, null otherwise.</returns>
    public async Task<DisconnectResponse?> Disconnect()
    {
        Debug.Assert(App.SessionId != null, "Cannot disconnect while not connected.");

        using var httpClient = CreateHttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(new { sessionId = App.SessionId, applicationId = App.Metadata.ApplicationId }), System.Text.Encoding.UTF8, "application/json");

        if (App.EnableDebugLogging)
            Console.Error.WriteLine($"Sending POST request to: /api/v1/disconnect with content: {await content.ReadAsStringAsync()}");

        var response = await httpClient.PostAsync("/api/v1/disconnect", content);
        return await HandleResponse<DisconnectResponse>(response, ApiStatus.Disconnected);
    }

    /// <summary>
    /// Retrieves a feature from the API.
    /// </summary>
    /// <param name="featureName">The name of the feature to retrieve.</param>
    /// <returns>A Feature object if found, null otherwise.</returns>
    public async Task<Feature?> GetFeature(string featureName)
    {
        Debug.Assert(App.SessionId != null, "You are attempting to call GetFeature() without being connected.");

        using var httpClient = CreateHttpClient();

        var encodedSessionId = HttpUtility.UrlEncode(App.SessionId);
        var encodedApplicationId = HttpUtility.UrlEncode(App.Metadata.ApplicationId);
        var encodedFeatureName = HttpUtility.UrlEncode(featureName);

        var getUrl = $"api/v1/feature?sessionId={encodedSessionId}&applicationId={encodedApplicationId}&name={encodedFeatureName}";

        if (App.EnableDebugLogging)
            Console.Error.WriteLine($"Sending GET request to: {getUrl}");

        var response = await httpClient.GetAsync(getUrl);
        return await HandleResponse<Feature>(response, ApiStatus.Ok);
    }

    /// <summary>
    /// Connects to the API using a license key.
    /// </summary>
    /// <param name="license">The license key to use for connection.</param>
    /// <param name="options">Optional connection options.</param>
    /// <returns>A ConnectResponse object if successful, null otherwise.</returns>
    public async Task<ConnectResponse?> Connect(string license, ConnectOptions? options = null)
    {
        options ??= new ConnectOptions();

        using var httpClient = CreateHttpClient();
        HttpResponseMessage? response = null;
        int numberOfAttempts = 0;

        while (response == null)
        {
            ++numberOfAttempts;

            try
            {
                var requestContent = new StringContent(JsonConvert.SerializeObject(new { licenseKey = license, applicationId = App.Metadata.ApplicationId }), System.Text.Encoding.UTF8, "application/json");

                if (App.EnableDebugLogging)
                    Console.Error.WriteLine($"Sending POST request to: /api/v1/connect (Attempt {numberOfAttempts}) with content: {await requestContent.ReadAsStringAsync()}");

                response = await httpClient.PostAsync("/api/v1/connect", requestContent);
            }
            catch (HttpRequestException httpRequestException)
            {
                if (App.EnableDebugLogging)
                    Console.Error.WriteLine($"HTTP request exception on attempt {numberOfAttempts}: {httpRequestException.Message}");

                if (httpRequestException.InnerException is System.Net.Sockets.SocketException)
                {
                    if (numberOfAttempts >= options.MaxRetries)
                    {
                        if (App.EnableDebugLogging)
                            Console.Error.WriteLine($"Max retries ({options.MaxRetries}) reached. Throwing ConnectionException.");
                        throw new ConnectionException($"The server failed to connect. ({httpRequestException.Message})");
                    }
                }
                return null;
            }
        }

        var connectResponse = await HandleResponse<ConnectResponse>(response, ApiStatus.Ok) ?? throw new JsonException("The server sent incorrect or poorly formatted JSON content.");
        options.PostConnect?.Invoke(connectResponse);
        return connectResponse;
    }
}