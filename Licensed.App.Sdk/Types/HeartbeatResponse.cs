using Newtonsoft.Json;

namespace Licensed.App.Sdk.Types;

public class HeartbeatResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }
}
