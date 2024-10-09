using Newtonsoft.Json;

namespace Licensed.App.Sdk;

public class ErrorResponse
{
    [JsonProperty(PropertyName = "error")]
    public string Message { get; set; } = "This message contains no content.";
}
