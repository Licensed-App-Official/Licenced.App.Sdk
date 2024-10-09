using Newtonsoft.Json;
using System.Text;

using System.Net;

namespace Licensed.App.Sdk.Types;

/// <summary>
/// Options to modify the behaviour of <see cref="RequestManager.Connect(string)"/> or <see cref="LicensedApp.Connect(string)"/>.
/// </summary>
public class ConnectOptions
{
    /// <summary>
    /// How many times should we retry the connection if the server refuses? (Default: 3)
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// This function, if not null, will be executed after a response is succesfully received.
    /// </summary>
    public Action<ConnectResponse>? PostConnect { get; set; }
}

/// <summary>
/// Internally used as a wrapper in the call to <see cref="HttpClient.PostAsync(string?, HttpContent?)"/>.
/// See <see cref="ConnectRequest.Request"/> for actual details.
/// </summary>
public class ConnectRequest : HttpContent
{
    public class Request
    {
        [JsonProperty(PropertyName = "applicationId")]
        public required string ApplicationId { get; init; }

        [JsonProperty(PropertyName = "licenseKey")]
        public required string License { get; init; }
    }

    private readonly string _jsonContent;
    private readonly Request _request;

    public Request GetRequest() => _request;

    public ConnectRequest(string applicationId, string license)
    {
        _jsonContent = JsonConvert.SerializeObject(_request = new Request()
        {
            ApplicationId = applicationId,
            License = license
        });
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        await stream.WriteAsync(Encoding.UTF8.GetBytes(_jsonContent));
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _jsonContent.Length;
        return true;
    }
}

/// <summary>
/// This is the response structure that is returned from the API endpoint
/// <code>/api/v1/connect</code>
/// </summary>
public class ConnectResponse
{
    /// <summary>
    /// The session identifier for this client. They must provide heartbeats or this will expire.
    /// </summary>
    [JsonProperty(PropertyName = "sessionId")]
    public string? SessionId { get; internal set; }

    /// <summary>
    /// When the license the user has connected with will expire.
    /// </summary>
    [JsonProperty(PropertyName = "expiration")]
    public DateTime? ExpirationTime { get; init; }

    /// <summary>
    /// The users total license length in days. Even if there's 1 minute remaining, this will describe
    /// the amount of time this license had in total.
    /// </summary>
    [JsonProperty(PropertyName = "length")]
    public long Length { get; }

    [JsonProperty(PropertyName = "applicationName")]
    public string? ApplicationName { get; }
}
