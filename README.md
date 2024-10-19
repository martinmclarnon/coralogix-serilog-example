# Coralogix Serilog Example

This project demonstrates how to integrate [Serilog](https://serilog.net/) with [Coralogix](https://coralogix.com/) for logging in a .NET Core 7.0 application. It shows how to configure Serilog, capture and send logs to Coralogix, and manage environment-specific configurations using an `appsettings.json` files.

## Table of Contents
- [Getting Started](#getting-started)
- [Requirements](#requirements)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [Environment Setup](#environment-setup)
- [License](#license)

## Getting Started

This project uses Serilog for logging and demonstrates how to send logs to Coralogix. It includes configuration for different environments, allowing the app to load environment-specific settings.

### Requirements

- .NET Core 7.0 SDK or later
- Coralogix Account & API Key
- Visual Studio Code (optional but recommended)
- [Serilog](https://serilog.net/) and [Serilog.Sinks.Console](https://github.com/serilog/serilog-sinks-console) NuGet packages
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/) NuGet package

### Installing Packages

Run the following command to install the required NuGet packages:

```bash
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Newtonsoft.Json
```

### Configuration

The application uses appsettings.json for default configurations and appsettings.{Environment}.json for environment-specific settings.

appsettings.json
```json
{
    "Coralogix": {
      "ApiKey": "[CORALOGIX_API_KEY]",  
      "BearerToken": "Bearer [CORALOGIX_API_KEY]",  
      "ApplicationName": "[CORALOGIX_APPLICATION_NAME]",
      "SubsystemName": "[CORALOGIX_APPLICATION_SUBSYSTEM_NAME]",
      "LogsV1APIEndpoint": "[CORALOGIX_LOGS_V1_API_ENDPOINT]"
    },
    "LoggingMessages": {
      "VerboseMessage": "This is a verbose log message.",
      "ErrorMessage": "This is an error log message.",
      "InformationMessage": "This is an information log message for Coralogix!"
    },
    "LogEntries": {
      "VerboseLog": "Verbose log sent to Coralogix.",
      "InformationLog": "Information log sent to Coralogix.",
      "WarningLog": "Warning log sent to Coralogix.",
      "ErrorLog": "Error log sent to Coralogix."
    }
  }
```

### Running the Application

You can run the application using the dotnet run command. If you're developing locally, make sure the environment is set to Development to load appsettings.local-development.json.

For macOS/Linux:
```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### Environment Setup
The project supports environment-specific configurations using the ASPNETCORE_ENVIRONMENT environment variable. It looks for the following files:

appsettings.json: The default settings file.
appsettings.{Environment}.json: Overrides specific settings based on the environment.
appsettings.local-development.json: Contains local development settings.
If the environment is set to "Development", the application will load appsettings.local-development.json in addition to appsettings.json.

### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.