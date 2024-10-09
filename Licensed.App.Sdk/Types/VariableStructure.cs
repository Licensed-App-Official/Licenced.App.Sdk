using Newtonsoft.Json;

namespace Licensed.App.Sdk.Types;

/// <summary>
/// This is the response structure that is returned from the API endpoint
/// <code>/api/v1/variable</code>
/// </summary>
public class Variable
{
    [JsonProperty(PropertyName = "key")]
    public string Name { get; internal set; } = string.Empty;

    [JsonProperty(PropertyName = "value")]
    public string Value { get; internal set; } = string.Empty;
}
