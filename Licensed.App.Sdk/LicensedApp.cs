using Licensed.App.Sdk.Exceptions;
using Licensed.App.Sdk.Types;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Threading;

namespace Licensed.App.Sdk;

/// <summary>
/// Represents the main class for interacting with the Licensed App SDK.
/// This class manages the connection, session, and API requests to the licensing server.
/// </summary>
public class LicensedApp : IDisposable
{
    /// <summary>
    /// Gets the metadata for web requests, including base URL and application ID.
    /// </summary>
    public WebMetadata Metadata { get; }

    /// <summary>
    /// Gets the RequestManager responsible for handling API requests.
    /// </summary>
    public RequestManager RequestManager { get; }

    /// <summary>
    /// Gets or sets the session ID. This is generated when connecting and is unique to each user.
    /// This is required to make requests to your application.
    /// </summary>
    public string? SessionId { get; internal set; }

    /// <summary>
    /// Gets or sets the flag that enables debug logging. If true, many functions will log information to
    /// <see cref="Console.Error"/>.
    /// </summary>
    public bool EnableDebugLogging { get; set; }

    /// <summary>
    /// Gets or sets the exception handler. If this is null, exceptions are thrown. 
    /// If it is not null, exceptions are passed into this function and not thrown.
    /// </summary>
    public RequestExceptionHandler? ExceptionHandler
    {
        get => RequestManager.ExceptionHandler;
        set => RequestManager.ExceptionHandler = value;
    }

    /// <summary>
    /// Gets or sets the heartbeat thread. This thread relies on the main thread, due to using <c>this</c>.
    /// </summary>
    private Thread? HeartbeatThread { get; set; }

    /// <summary>
    /// ManualResetEvent to signal the heartbeat thread to stop.
    /// </summary>
    private ManualResetEvent StopHeartbeatEvent { get; } = new ManualResetEvent(false);

    /// <summary>
    /// Initializes a new instance of the LicensedApp class. 
    /// This will not make any requests initially; it just initializes internals.
    /// </summary>
    /// <param name="applicationId">Your application ID. This will be on your dashboard.</param>
    public LicensedApp(string applicationId)
    {
        Metadata = new WebMetadata("http://localhost:3000", "api/v1", applicationId);
        RequestManager = new RequestManager(this);
    }

    /// <summary>
    /// Gets a value indicating whether the application is connected.
    /// True if there's a SessionId and the heartbeat thread is still running.
    /// </summary>
    public bool Connected => (SessionId != null && HeartbeatThread != null && HeartbeatThread.IsAlive);

    /// <summary>
    /// Connects to the application using a license.
    /// </summary>
    /// <param name="license">The license key, which must be valid from your panel.</param>
    /// <param name="options">Optional connection options.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ApplicationNotFoundException">Thrown when the application is not found.</exception>
    /// <exception cref="ApplicationNotSetupException">Thrown when the application is not properly set up.</exception>
    /// <exception cref="InternalServerErrorException">Thrown when an internal server error occurs.</exception>
    /// <exception cref="LicenseBannedException">Thrown when the license is banned.</exception>
    /// <exception cref="LicenseExpiredException">Thrown when the license has expired.</exception>
    /// <exception cref="LicensePausedException">Thrown when the license is paused.</exception>
    /// <exception cref="RateLimitExceededException">Thrown when the rate limit is exceeded.</exception>
    /// <exception cref="UnauthenticatedException">Thrown when authentication fails.</exception>
    /// <exception cref="JsonException">Thrown when there's an error parsing the JSON response.</exception>
    public async Task<ConnectResponse?> Connect(string license, ConnectOptions? options = null)
    {
        var response = await RequestManager.Connect(license, options);
        if (response == null)
        {
            // An exception handler dealt with it, as no exception occurred.
            return null;
        }
        SessionId = response.SessionId;
        if (HeartbeatThread == null || !HeartbeatThread.IsAlive)
        {
            StopHeartbeatEvent.Reset();
            HeartbeatThread = new Thread(HeartbeatFunction);
            HeartbeatThread.Start();
        }
        return response;
    }

    /// <summary>
    /// Disconnects from the application. This will nullify the session id, meaning this client cannot 
    /// use the API anymore unless <see cref="Connect(string, ConnectOptions?)"/> is called again. 
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="FailedToDisconnectException">Thrown when the disconnection process fails.</exception>
    /// <exception cref="ApplicationNotFoundException">Thrown when the application is not found.</exception>
    /// <exception cref="ApplicationNotSetupException">Thrown when the application is not properly set up.</exception>
    /// <exception cref="InternalServerErrorException">Thrown when an internal server error occurs.</exception>
    /// <exception cref="RateLimitExceededException">Thrown when the rate limit is exceeded.</exception>
    /// <exception cref="UnauthenticatedException">Thrown when authentication fails.</exception>
    /// <exception cref="JsonException">Thrown when there's an error parsing the JSON response.</exception>
    public async Task Disconnect()
    {
        Debug.Assert(SessionId != null, "Cannot call Disconnect() without being connected.");

        var response = await RequestManager.Disconnect();
        if (response == null)
        {
            // An exception handler dealt with it, as no exception occurred.
            return;
        }
        bool disconnected = response.Success;
        if (!disconnected)
            throw new FailedToDisconnectException("Failed to disconnect from the server.");

        SessionId = null;
        StopHeartbeatEvent.Set();

        if (HeartbeatThread != null && HeartbeatThread.IsAlive)
        {
            if (!HeartbeatThread.Join(5000))
            {
                if (EnableDebugLogging)
                    Console.Error.WriteLine("Heartbeat thread did not stop within the timeout period.");
            }
        }
    }

    /// <summary>
    /// Retrieves a feature from the server.
    /// </summary>
    /// <param name="featureName">The name of the feature to retrieve.</param>
    /// <returns>A task representing the asynchronous operation, containing the Feature object if found, or null otherwise.</returns>
    /// <exception cref="ApplicationNotFoundException">Thrown when the application is not found.</exception>
    /// <exception cref="ApplicationNotSetupException">Thrown when the application is not properly set up.</exception>
    /// <exception cref="InternalServerErrorException">Thrown when an internal server error occurs.</exception>
    /// <exception cref="RateLimitExceededException">Thrown when the rate limit is exceeded.</exception>
    /// <exception cref="UnauthenticatedException">Thrown when authentication fails.</exception>
    /// <exception cref="JsonException">Thrown when there's an error parsing the JSON response.</exception>
    public async Task<Feature?> GetFeature(string featureName)
    {
        Debug.Assert(SessionId != null, "Cannot call GetFeature() without being connected.");
        return await RequestManager.GetFeature(featureName);
    }

    /// <summary>
    /// Retrieves a variable from the server.
    /// </summary>
    /// <param name="variableName">The name of the variable to retrieve.</param>
    /// <returns>A task representing the asynchronous operation, containing the Variable object if found, or null otherwise.</returns>
    /// <exception cref="ApplicationNotFoundException">Thrown when the application is not found.</exception>
    /// <exception cref="ApplicationNotSetupException">Thrown when the application is not properly set up.</exception>
    /// <exception cref="InternalServerErrorException">Thrown when an internal server error occurs.</exception>
    /// <exception cref="RateLimitExceededException">Thrown when the rate limit is exceeded.</exception>
    /// <exception cref="UnauthenticatedException">Thrown when authentication fails.</exception>
    /// <exception cref="JsonException">Thrown when there's an error parsing the JSON response.</exception>
    public async Task<Variable?> GetVariable(string variableName)
    {
        Debug.Assert(SessionId != null, "Cannot call GetVariable() without being connected.");
        return await RequestManager.GetVariable(variableName);
    }

    /// <summary>
    /// The function responsible for sending heartbeats to the server.
    /// This method is executed in a separate thread.
    /// </summary>
    private void HeartbeatFunction()
    {
        const int heartbeatInterval = 60000; // 60 seconds
        const int maxFailedAttempts = 3;

        int failedAttempts = 0;

        while (!StopHeartbeatEvent.WaitOne(0))
        {
            if (SessionId == null)
            {
                if (EnableDebugLogging)
                    Console.Error.WriteLine("Heartbeat thread stopping due to null SessionId");
                break;
            }

            try
            {
                bool success = Task.Run(async () => await RequestManager.SendHeartbeat()).Result;

                if (success)
                {
                    failedAttempts = 0;
                    if (EnableDebugLogging)
                        Console.Error.WriteLine("Heartbeat sent successfully");
                }
                else
                {
                    failedAttempts++;
                    if (EnableDebugLogging)
                        Console.Error.WriteLine($"Failed to send heartbeat. Attempt {failedAttempts} of {maxFailedAttempts}");
                }

                if (failedAttempts >= maxFailedAttempts)
                {
                    if (EnableDebugLogging)
                        Console.Error.WriteLine("Max failed heartbeat attempts reached. Disconnecting.");
                    Task.Run(async () => await Disconnect()).Wait();
                    break;
                }
            }
            catch (Exception ex)
            {
                if (EnableDebugLogging)
                    Console.Error.WriteLine($"Error in heartbeat thread: {ex.Message}");
                failedAttempts++;
            }

            // Wait for the next interval or until the stop event is signaled
            StopHeartbeatEvent.WaitOne(heartbeatInterval);
        }

        if (EnableDebugLogging)
            Console.Error.WriteLine("Heartbeat thread stopped.");
    }

    /// <summary>
    /// Releases the unmanaged resources used by the LicensedApp and optionally releases the managed resources.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        StopHeartbeatEvent.Set();

        if (SessionId is null)
        {
            if (HeartbeatThread != null && HeartbeatThread.IsAlive)
                HeartbeatThread.Join(5000);
            return;
        }

        try
        {
            Task.Run(async () =>
            {
                await Disconnect();
            }).Wait();
        }
        catch (FailedToDisconnectException)
        {
            // TODO: Consider logging this exception or handling it in some way
        }
        finally
        {
            StopHeartbeatEvent.Dispose();
        }
    }
}