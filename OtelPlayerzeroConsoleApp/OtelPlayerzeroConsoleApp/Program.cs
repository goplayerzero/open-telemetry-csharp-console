using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;

namespace OtelPlayerzeroConsoleApp
{
    class Program
    {
        private static ActivitySource activitySource;
        private static TracerProvider tracerProvider;
        private static MeterProvider meterProvider;
        private static ILoggerFactory loggerFactory;

        private static ILogger<Program> logger;

        static void Main(string[] args)
        {
            var serviceName = "p0-console-4.6.2";
            var serviceVersion = "1.0.0";
            
            var endPoint = "https://sdk.playerzero.app/otlp";
            var headers = "Authorization=Bearer 666af2fef6b93a24518cf726,x-pzprod=true";
            
            var resourceBuilder = ResourceBuilder.CreateDefault()
               .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

            tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddSource(serviceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endPoint + "/v1/traces");
                    options.Headers = headers;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .AddConsoleExporter()
                .Build();

            meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddMeter(serviceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endPoint + "/v1/metrics");
                    options.Headers = headers;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .AddConsoleExporter()
                .Build();

            // Configure logging
            loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddOpenTelemetry(logging =>
                    {
                        logging.IncludeFormattedMessage = true;
                        logging.IncludeScopes = true;
                        logging.ParseStateValues = true;
                        logging.SetResourceBuilder(resourceBuilder);
                        logging.AddConsoleExporter();
                        logging.AddOtlpExporter(options =>
                         {
                             options.Endpoint = new Uri(endPoint + "/v1/logs");
                             options.Headers = headers;
                             options.Protocol = OtlpExportProtocol.HttpProtobuf;
                         });
                    });
            });

            logger = loggerFactory.CreateLogger<Program>();

            activitySource = new ActivitySource(serviceName);

            using (var activity = Activity.Current ?? activitySource.StartActivity("Main"))
            {
                if (activity != null)
                {
                    // Log a message
                    logger.LogInformation("Open Telemetry integration with Playerzero.");
                    logger.LogError("Logging error.");

                    // Simulate user service
                    var userService = new UserService(loggerFactory.CreateLogger<UserService>());

                    // Simulate user login
                    SimulateUserLogin(userService);
                }
            }

            logger.LogTrace("Logging trace");

            Console.WriteLine("Hello, World!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            DisposeProviders();
        }

        private static void DisposeProviders()
        {
            tracerProvider?.Dispose();
            meterProvider?.Dispose();
            loggerFactory?.Dispose();
        }

        static void SimulateUserLogin(UserService userService)
        {
            string username = "foo";
            string password = "bar";

            if (userService.Authenticate(username, password))
            {
                Console.WriteLine("User logged in successfully!");
            }
            else
            {
                Console.WriteLine("Login failed. Please check your credentials.");
            }
        }
    }
}