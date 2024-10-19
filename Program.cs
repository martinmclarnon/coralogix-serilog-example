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

            // Load the default appsettings.json
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Load the environment-specific file (now guaranteed to be Development)
            config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

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