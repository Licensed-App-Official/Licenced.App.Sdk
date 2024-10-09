
/*
 * Welcome to the Licensed.App C# example.
 * 
 * Please ensure that you have added our nuget package, or alternatively,
 * compiled the project yourself and added the DLL as a reference.
 * 
 * This will serve as a base for any application using Licensed.App with C#.
 * 
 * NOTE: We will be using the top-level statements features to allow seamless async/await syntax.
 *       However, if you do have a `Main()` function, you must change the signature for the function
 *       to this:
 *       
 *  ----------------------
 *  public async Task Main() { ... }
 *  ----------------------
 *  
 *  This project references `Licensed.App.Sdk` from this solution.
 *  We also use `Spectre.Console` from nuget. This is not required, but just for style.
 */
using Licensed.App.Sdk;
using Spectre.Console;


const string applicationId = "4129b0d2-5ccb-419d-819f-645f359358c5";

// Initialize the application using your application id.
// If you don't call Disconnect() manually, make sure to make this a `using var`!

// If you don't correctly shutdown and log the user out, they will get locked out for some time
// before their session expires.
using var application = new LicensedApp(applicationId)
{
    // Set the exception handler. This is not always recommended, but it works here to output information
    // easily. This will only catch exceptions that derive from LicensingException.
    ExceptionHandler = (e, status) =>
    {
        AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
    }
};

var userInput = AnsiConsole.Ask<string>("License: ");
var connectionResponse = await application.Connect(userInput);

// The user could not connect. (Described by the exceptions, usually)
// However, it will not throw, because application.ExceptionHandler is set.
if (connectionResponse == null)
{
    return;
}

// At this point, the heartbeat thread has been launched automatically.

// Please note, connectionResponse.ApplicationName will be null if your application settings request this is not sent.
AnsiConsole.MarkupLine($"Successfully connected to [green bold]{connectionResponse.ApplicationName ?? "(redacted)"}[/].");
var daysUntilExpiration = (connectionResponse.ExpirationTime - DateTime.UtcNow)?.Days ?? 0;
AnsiConsole.MarkupLine($"! You have [yellow italic]{daysUntilExpiration}[/] days left of your license.");

var isApplicationEnabledFeature = await application.GetFeature("enabled")
    ?? throw new Exception("Failed to get a response from the server to decide whether or not the application is enabled.");

AnsiConsole.MarkupLine("Enabled? {0}", isApplicationEnabledFeature.Enabled ? "[green]Yes[/]" : "[red]No[/]");

var messageOfTheDay = (await application.GetVariable("motd"))?.Value ?? "Unknown MOTD";

var rule = new Rule()
{
    Justification = Justify.Center
};

rule.Title = "Message Of The Day";

AnsiConsole.Write(rule);
rule.Title = messageOfTheDay;
AnsiConsole.Write(rule);