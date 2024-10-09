using Newtonsoft.Json;

namespace Licensed.App.Sdk.Types;

/// <summary>
/// This is the response structure that is returned from the API endpoint
/// <code>/api/v1/feature</code>
/// </summary>
public class Feature
{
    [JsonProperty(PropertyName = "name")]
    public string Name { get; internal set; } = string.Empty;

    [JsonProperty(PropertyName = "enabled")]
    public bool Enabled { get; internal set; } = false;
}