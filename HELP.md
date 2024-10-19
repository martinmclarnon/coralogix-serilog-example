### Basic information log to send test message configuring Serilog ###
GOTCHA!!! Ensure to flush logs before exiting the application on line 21.

```shell
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
```

```csharp
class Program
{
    static void Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.OpenTelemetry(options => {
                options.Endpoint = "[CORALOGIX_LOGS_V1_API_ENDPOINT]";
                options.Headers = new Dictionary<string, string>{
                         {"Authorization", "Bearer [CORALOGIX_API_KEY]"},
                         {"CX-Application-Name", "[CORALOGIX_APPLICATION_NAME]"},
                         {"CX-Subsystem-Name", "[CORALOGIX_APPLICATION_SUBSYSTEM_NAME]"}
                };
            })
            .CreateLogger();

        Log.Information("This is a test log for Coralogix from MFOMCLARNON!");
        Log.CloseAndFlush();
    }
}
```

### Rewrite the basic code to send all log message types ###

- Configure Serilog with Console sink for local debugging
- Capture all levels including Verbose
- Write logs to Console
- Log messages at different levels
- Send multiple logs to Coralogix
    - Create multiple log entries for different severity levels
    - Coralogix API log structure
    - Convert log data to JSON
    - Add necessary headers for Coralogix API
    - Send logs to Coralogix
- Ensure logs are flushed before exiting


```shell
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet add package Newtonsoft.Json
```

```csharp
class Program
{
    static async Task Main(string[] args)
    {
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()  
            .WriteTo.Console()       
            .CreateLogger();
        
        Log.Verbose("This is a verbose log message.");
        Log.Error("This is an error log message.");
        Log.Information("This is an information log message for Coralogix!");

        await SendMultipleLogsToCoralogix();

        Log.CloseAndFlush();
    }

    static async Task SendMultipleLogsToCoralogix()
    {
        var client = new HttpClient();

        // Create multiple log entries for different severity levels
        var logEntries = new[]
        {
            new { severity = 5, text = "Verbose log sent to Coralogix from mfomclarnon." },  // Severity 5 = Verbose
            new { severity = 3, text = "Information log sent to Coralogix from mfomclarnon." },  // Severity 3 = Information
            new { severity = 4, text = "Warning log sent to Coralogix from mfomclarnon." },  // Severity 4 = Warning
            new { severity = 2, text = "Error log sent to Coralogix from mfomclarnon." }  // Severity 2 = Error
        };

        // Coralogix API log structure
        var logData = new
        {
            privateKey = "[CORALOGIX_API_KEY]",  
            applicationName = "[CORALOGIX_APPLICATION_NAME]",  // Application name in Coralogix
            subsystemName = "[CORALOGIX_APPLICATION_SUBSYSTEM_NAME]",  // Subsystem name in Coralogix
            logEntries = logEntries
        };

        // Convert log data to JSON
        var content = new StringContent(JsonConvert.SerializeObject(logData), Encoding.UTF8, "application/json");

        // Add necessary headers for Coralogix API
        client.DefaultRequestHeaders.Add("Authorization", "Bearer [CORALOGIX_API_KEY]");  
        client.DefaultRequestHeaders.Add("CX-Application-Name", "[CORALOGIX_APPLICATION_NAME]");
        client.DefaultRequestHeaders.Add("CX-Subsystem-Name", "[CORALOGIX_APPLICATION_SUBSYSTEM_NAME]");

        // Send logs to Coralogix
        var response = await client.PostAsync("[CORALOGIX_LOGS_V1_API_ENDPOINT]", content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Logs sent successfully!");
        }
        else
        {
            Console.WriteLine("Failed to send logs: " + response.StatusCode);
        }
    }
}
```

### Rewrite the additional code sending all log message types, to include Metrics and Tracing and add a local development appsettings file ###

- Resolve IConfiguration from the host's service provider
- Force the environment to "Development" for debugging purposes
- Load the local-development file if we are in a local environment
-  Extract values from the config
- Configure Serilog with Console sink for local debugging
- Log messages at different levels
- Capture and log metrics and tracing
- Send multiple logs to Coralogix
    - Create multiple log entries for different severity levels
    - Coralogix API log structure
    - Convert log data to JSON
    - Add necessary headers for Coralogix API
    - Send logs to Coralogix
- Ensure logs are flushed before exiting


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

```shell
dotnet add package System.Diagnostics.DiagnosticSource
dotnet add package System.Text.Encoding
dotnet add package Newtonsoft.Json
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.Extensions.DependencyInjection
```

```csharp
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static async Task Main(string[] args)
    {        
        Console.WriteLine($"ASPNETCORE_ENVIRONMENT = {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

        var host = CreateHostBuilder(args).Build();

        // Resolve IConfiguration from the host's service provider
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var startup = new Startup(configuration);

        await startup.StartApplication();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            // Force the environment to "Development" for debugging purposes
            hostingContext.HostingEnvironment.EnvironmentName = "Development";
            Console.WriteLine($"Forcing environment to: {hostingContext.HostingEnvironment.EnvironmentName}");  // Debugging

            // Load the local-development file if we are in a local environment
            if (hostingContext.HostingEnvironment.IsDevelopment())
            {
                config.AddJsonFile("appsettings.local-development.json", optional: true, reloadOnChange: true);
                Console.WriteLine("Loaded appsettings.local-development.json");  // Debugging
            }

            // Add environment variables
            config.AddEnvironmentVariables();
        });
}

public class Startup
{
    private readonly IConfiguration _config;

    public Startup(IConfiguration config)
    {
        _config = config;
    }

    public async Task StartApplication()
    {
        // Extract values from the config
        var coralogixSettings = _config.GetSection("Coralogix");
        var loggingMessages = _config.GetSection("LoggingMessages");
        var logEntriesConfig = _config.GetSection("LogEntries");

        string coralogixApiKey = coralogixSettings["ApiKey"];
        string bearerToken = coralogixSettings["BearerToken"];
        string applicationName = coralogixSettings["ApplicationName"];
        string subsystemName = coralogixSettings["SubsystemName"];
        string coralogixLogsV1APIEndpoint = coralogixSettings["LogsV1APIEndpoint"];

        string verboseMessage = loggingMessages["VerboseMessage"];
        string errorMessage = loggingMessages["ErrorMessage"];
        string informationMessage = loggingMessages["InformationMessage"];

        // Configure Serilog with Console sink for local debugging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()  // Capture all levels including Verbose
            .WriteTo.Console()       // Write logs to Console
            .CreateLogger();

        // Log messages at different levels
        Log.Verbose(verboseMessage);
        Log.Error(errorMessage);
        Log.Information(informationMessage);

        // Capture and log metrics and tracing
        await CaptureMetricsAndTracing(coralogixApiKey, bearerToken, applicationName, subsystemName, coralogixLogsV1APIEndpoint);

        // Send multiple logs to Coralogix
        await SendMultipleLogsToCoralogix(coralogixApiKey, bearerToken, applicationName, subsystemName, coralogixLogsV1APIEndpoint, logEntriesConfig);

        // Ensure logs are flushed before exiting
        Log.CloseAndFlush();
    }

    static async Task CaptureMetricsAndTracing(string apiKey, string bearerToken, string applicationName, string subsystemName, string coralogixLogsV1APIEndpoint)
    {
        var client = new HttpClient();

        // Example of capturing some metrics
        var totalMemory = GC.GetTotalMemory(false);
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

        // Example of tracing an operation
        var activity = new Activity("SampleOperation");
        activity.Start();
        try
        {
            // Simulate some operation
            await Task.Delay(1000);
        }
        finally
        {
            activity.Stop();
        }

        // Create log entries for metrics and tracing
        var logEntries = new[]
        {
            new { severity = 3, text = $"System metrics: Total Memory: {totalMemory} bytes, Uptime: {uptime}" },
            new { severity = 3, text = $"Operation {activity.OperationName} took {activity.Duration.TotalMilliseconds} ms" }
        };

        // Coralogix API log structure
        var logData = new
        {
            privateKey = apiKey,
            applicationName = applicationName,
            subsystemName = subsystemName,
            logEntries = logEntries
        };

        // Convert log data to JSON
        var content = new StringContent(JsonConvert.SerializeObject(logData), Encoding.UTF8, "application/json");

        // Add necessary headers for Coralogix API
        client.DefaultRequestHeaders.Add("Authorization", bearerToken);
        client.DefaultRequestHeaders.Add("CX-Application-Name", applicationName);
        client.DefaultRequestHeaders.Add("CX-Subsystem-Name", subsystemName);

        // Send logs to Coralogix
        var response = await client.PostAsync(coralogixLogsV1APIEndpoint, content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Metrics and tracing data sent successfully!");
        }
        else
        {
            Console.WriteLine("Failed to send data: " + response.StatusCode);
        }
    }

    static async Task SendMultipleLogsToCoralogix(string apiKey, string bearerToken, string applicationName, string subsystemName, string coralogixLogsV1APIEndpoint, IConfigurationSection logEntriesConfig)
    {
        var client = new HttpClient();

        // Create log entries using the values from configuration
        var verboseLogEntry = new { severity = 5, text = logEntriesConfig["VerboseLog"] };
        var informationLogEntry = new { severity = 3, text = logEntriesConfig["InformationLog"] };
        var warningLogEntry = new { severity = 4, text = logEntriesConfig["WarningLog"] };
        var errorLogEntry = new { severity = 2, text = logEntriesConfig["ErrorLog"] };

        // Create multiple log entries for different severity levels
        var logEntries = new[] { verboseLogEntry, informationLogEntry, warningLogEntry, errorLogEntry };

        // Coralogix API log structure
        var logData = new
        {
            privateKey = apiKey,
            applicationName = applicationName,
            subsystemName = subsystemName,
            logEntries = logEntries
        };

        // Convert log data to JSON
        var content = new StringContent(JsonConvert.SerializeObject(logData), Encoding.UTF8, "application/json");

        // Add necessary headers for Coralogix API
        client.DefaultRequestHeaders.Add("Authorization", bearerToken);
        client.DefaultRequestHeaders.Add("CX-Application-Name", applicationName);
        client.DefaultRequestHeaders.Add("CX-Subsystem-Name", subsystemName);

        // Send logs to Coralogix
        var response = await client.PostAsync(coralogixLogsV1APIEndpoint, content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Logs sent successfully!");
        }
        else
        {
            Console.WriteLine("Failed to send logs: " + response.StatusCode);
        }
    }
}
```