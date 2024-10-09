using System;

namespace Licensed.App.Sdk;

/// <summary>
/// Represents the rate limiting information for API requests.
/// </summary>
public class RateLimits
{
    /// <summary>
    /// Gets the maximum number of requests allowed within the rate limit window.
    /// </summary>
    public int Limit { get; internal set; }

    /// <summary>
    /// Gets the number of requests remaining within the current rate limit window.
    /// </summary>
    public int Remaining { get; internal set; }

    /// <summary>
    /// Gets the date and time when the current rate limit window resets.
    /// </summary>
    public DateTime Reset { get; internal set; }

    /// <summary>
    /// Updates the rate limit information with new values.
    /// </summary>
    /// <param name="limit">The maximum number of requests allowed.</param>
    /// <param name="remaining">The number of requests remaining.</param>
    /// <param name="reset">The date and time when the rate limit resets.</param>
    public void Fill(int limit, int remaining, DateTime reset)
    {
        Limit = limit;
        Remaining = remaining;
        Reset = reset;
    }
}

/// <summary>
/// Describes metadata about the actual webserver. This includes the base URL
/// and API base route. This class helps to facilitate development and release processes.
/// </summary>
public class WebMetadata
{
    /// <summary>
    /// Gets the base URL of the web server.
    /// </summary>
    public string BaseUrl { get; }

    /// <summary>
    /// Gets the base route for the API.
    /// </summary>
    public string ApiBase { get; }

    /// <summary>
    /// Gets the full base URL for API requests, combining BaseUrl and ApiBase.
    /// </summary>
    public string Base => $"{BaseUrl.TrimEnd('/')}/{ApiBase.TrimStart('/')}";

    /// <summary>
    /// Gets or sets the application ID associated with this metadata.
    /// </summary>
    public string ApplicationId { get; internal set; }

    /// <summary>
    /// Gets the active rate limits for the current client.
    /// </summary>
    public RateLimits RateLimits { get; }

    /// <summary>
    /// Initializes a new instance of the WebMetadata class.
    /// </summary>
    /// <param name="baseUrl">The base URL of the web server.</param>
    /// <param name="apiBase">The base route for the API.</param>
    /// <param name="applicationId">The application ID associated with this metadata.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null or empty.</exception>
    /// <exception cref="UriFormatException">Thrown if the baseUrl is not a valid URI.</exception>
    public WebMetadata(string baseUrl, string apiBase, string applicationId)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl), "Base URL cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(apiBase))
            throw new ArgumentNullException(nameof(apiBase), "API base cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(applicationId))
            throw new ArgumentNullException(nameof(applicationId), "Application ID cannot be null or empty.");

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
            throw new UriFormatException("The provided base URL is not a valid URI.");

        BaseUrl = baseUrl;
        ApiBase = apiBase;
        ApplicationId = applicationId;
        RateLimits = new RateLimits();
    }
}