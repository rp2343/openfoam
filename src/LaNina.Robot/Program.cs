using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using E2EStandard.Logging;
using LaNina.Robot.Engine;
using Microsoft.AzureCAT.Extensions.Logging.AppInsights;
using Microsoft.AzureCAT.Extensions.Logging.AppInsights.Initializer;
using Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoboCustos.Brain;
using RoboCustos.Brain.Schedulers;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LaNina.Robot
{
    internal static class Program
    {
        private static ILogger _logger;

        private static readonly CancellationTokenSource Cts = new CancellationTokenSource();

        private static void Main()
        {
            try
            {
                Console.CancelKeyPress += (e, args) => Cts.Cancel();

                string environmentVariable = Environment.GetEnvironmentVariable("ENVIRONMENT");
                string environmentName = string.IsNullOrEmpty(environmentVariable)
                    ? "local"
                    : environmentVariable;

                IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                    .SetBasePath($"{Directory.GetCurrentDirectory()}/settings")
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                    .Build();
                var rhapsoConfiguration = new LaNinaConfiguration(configurationRoot);

                // Because RoboCustos brain logs using serilog we need to configure that logging stuff here
                // we will use the information from the log appsettings.json to configure it

                // TODO: this is already in the "Robot" section which seems a little odd, maybe we should move it?

                CommonAzureSerilogConfiguration.ConfigureFromProvider(new NetCoreSettingsProvider(configurationRoot.GetSection("Robot")),
                    machineRole: "Robot",
                    config: new Configuration
                    {
                        ExtraConfigurationEnrichmentProperties = new[] { "TargetEnvironment" }
                    });

                ILoggerFactory loggerFactory = GetLoggerFactory(configurationRoot).Result;
                _logger = loggerFactory.CreateLogger(nameof(Robot));

                AssemblyLoadContext.Default.Unloading += context =>
                {
                    // Handle SIGTERM and trigger cancellation token
                    _logger.LogInformation("SIGTERM signal has been received...");
                    Log.Logger.Information("SIGTERM signal has been received... (Serilog)");
                    Cts.Cancel();
                };

                // Write interaction information to kusto

                var kustoLocker = new RoboCustos.Lockers.Kusto.KustoStorageLocker(_logger)
                {
                    IncludeHappinessCounts = rhapsoConfiguration.IncludeHappinessCounts
                };

                List<RoboStorageLocker> lockers = new List<RoboStorageLocker>();
                lockers.Add(kustoLocker);
                // When running debug (likely a dev machine) also stream things to the console
#if DEBUG
                lockers.Add(new ConsoleLocker());
#endif

                CombinedStorageLocker combined = new CombinedStorageLocker(lockers);

                IOffice office = CreateLaNinaOffice(rhapsoConfiguration, combined);
                // log to both logging apis to make sure they are both working
                _logger.LogInformation("Rhapso.Robot Starting...");
                Log.Logger.Information("Rhapso.Robot Starting... (Serilog)");

                // loop forever (not sure if this is actually the best plan, maybe we should let the process exit on errors and get re-started?)
                while (true)
                {
                    try
                    {
                        office.Work(Cts.Token).Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Exception encountered {e}");
                        if (e is TaskCanceledException || e is OperationCanceledException ||
                            (e is AggregateException && ((AggregateException)e).Flatten().InnerExceptions.Any(i => i is TaskCanceledException || i is OperationCanceledException)))
                        {
                            // we are being cancelled so exit
                            // right now the only way this gets called is through CTRL-C on dev machines but we may integrate
                            // with K8s pre-stop events later perhaps.
                            Log.Logger.Information("Robocustos execution cancelled.");
                            return;
                        }
                        else
                        {
                            // log the exception via both logging pipelines in case there is some issue with one of them
                            Log.Logger.Error(e, "Exception Encountered running office");
                            _logger.LogException(e, "Exception Encountered running office");
                        }
                    }
                }
            }
            finally
            {
                // make sure any serilog pipeline stuff gets flushed (not sure if we can do the same thing for the kusto pipeline)
                Log.CloseAndFlush();
            }
        }

        private static IOffice CreateLaNinaOffice(LaNinaConfiguration configuration, RoboStorageLocker storage)
        {
            var interactionRateProvider = new StaticInteractionRateProvider<LaNinaPersona>(
                configuration.WorkloadProfile.InteractionRates);

            var productInformation = new ProductTestedInformation(
                nameof(LaNina),
                configuration.TargetEnvironment,
                new Uri("http://lanina.com"),
                configuration.CloudPlatform);

            var interactionInformation = new InteractionInformation(productInformation, new AzureTesterInformation());
            var targetInteractionRatesPerMinute = interactionRateProvider.GetTypedInteractionRates(CancellationToken.None).Result;

            return new RoboOffice(storage, interactionInformation,
                targetInteractionRatesPerMinute.Select(kvp => LaNinaPersonaMother.MotherFor(kvp.Key, kvp.Value, _logger)),
                new SchedulerFactory(configuration.WorkloadProfile.Scheduler, interactionRateProvider),
                configuration.MaxDegreeOfParallelism,
                configuration.MaxMessagesPerTask
            );
        }

        private static async Task<ILoggerFactory> GetLoggerFactory(IConfigurationRoot configurationRoot)
        {
            var initializer = new ConfigurationTelemetryInitializer(configurationRoot);
            await AppInsightLoggingManager.Initialize(configurationRoot, initializer).ConfigureAwait(false);

            var factory = new LoggerFactory()
                .WithFilter(new FilterLoggerSettings
                {
                    {"Microsoft", LogLevel.Error},
                    {"System", LogLevel.Error}
                })
                .AddAppInsights(configurationRoot);

            return factory;
        }
    }
}