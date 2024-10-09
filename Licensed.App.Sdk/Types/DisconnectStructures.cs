using Newtonsoft.Json;
using System.Text;

using System.Net;

namespace Licensed.App.Sdk.Types;

public class DisconnectRequest : HttpContent
{
    internal class Request
    {
        [JsonProperty(PropertyName = "sessionId")]
        public required string SessionId { get; init; }

        [JsonProperty(PropertyName = "applicationId")]
        public required string ApplicationId { get; init; }
    }

    private readonly string _jsonContent;

    public DisconnectRequest(string sessionId, string applicationId)
    {
        _jsonContent = JsonConvert.SerializeObject(new Request()
        {
            SessionId = sessionId,
            ApplicationId = applicationId,
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
/// <code>/api/v1/disconnect</code>
/// </summary>
public class DisconnectResponse
{
    [JsonProperty(PropertyName = "success")]
    public bool Success { get; internal set; }
}