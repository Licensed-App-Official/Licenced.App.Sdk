# Licensed.App C# SDK

Licensed.App provides a powerful solution for managing software licenses and user sessions. This SDK offers a simple way to integrate with the Licensed.App API from your C# applications, allowing you to authenticate users, manage sessions, and retrieve features and variables from your license server.

## Features

- **Secure Authentication:** Validate licenses and manage sessions with ease.
- **Heartbeat System:** Automatically manage session validity through heartbeats.
- **Flexible API:** Access features and variables dynamically, tailored to your application's needs.

## Installation

Install the SDK via NuGet:

```bash
dotnet add package Licensed.App.Sdk
```

## Quick Start

Here's how you can get started with Licensed.App in just a few steps.

### 1. Initialize the LicensedApp SDK

Pass your `applicationId` from the Licensed.App dashboard:

```csharp
using Licensed.App.Sdk;

var licensedApp = new LicensedApp("your-application-id");
```

### 2. Connect Using a License Key

Authenticate your application with a valid license:

```csharp
string licenseKey = "your-license-key";
var response = await licensedApp.Connect(licenseKey);

if (response != null)
{
    Console.WriteLine($"Connected with session ID: {licensedApp.SessionId}");
}
```

### 3. Access Features and Variables

Once connected, you can access features and variables from your server:

```csharp
var feature = await licensedApp.GetFeature("premium-feature");
if (feature != null)
{
    Console.WriteLine($"Feature: {feature.Name}, Status: {feature.IsEnabled}");
}

var variable = await licensedApp.GetVariable("max-users");
if (variable != null)
{
    Console.WriteLine($"Variable: {variable.Name}, Value: {variable.Value}");
}
```

### 4. Disconnect

Ensure to disconnect when done:

```csharp
await licensedApp.Disconnect();
```

## Documentation

For more detailed information on all available methods and classes, please refer to the [Licensed.App Documentation](https://docs.licensed.app). If you encounter any issues, consult the docs or reach out for support.

---

That's it! You're now set up to manage licenses and sessions in your C# application with Licensed.App.
