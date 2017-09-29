using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LaNina.Robot.Engine.Benchmarks.Models;
using Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider;
using Microsoft.Extensions.Logging;
using RoboCustos.Brain;

namespace LaNina.Robot.Engine.Benchmarks
{
    // ReSharper disable once InconsistentNaming
    internal class HPCBenchmark: RoboPersona<LaNinaPersonaMother>
    {
        private readonly ILogger _logger;
        private const string LogFolderPathTemplate = "/var/log/hpc/deploy_robot.parameters_openfoam_benchmark_";

        #region Constructors

        public HPCBenchmark(LaNinaPersonaMother mother, ILogger logger) : base(mother)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        public override async Task<Interaction> InteractWithPlatform(CancellationToken? cancellationToken = null)
        {
            var builder = new AtomicInteractionBuilder<HPCBenchmarkResult>();

            HPCBenchmarkResult result;
            var benchmarkCorrelationId = Guid.NewGuid();
            try
            {
                var logFolderPath = $"{LogFolderPathTemplate}{benchmarkCorrelationId}";
                
                var bashResult = Bash($"automation/deploy.sh settings/robot.parameters.tpl automation/openfoam_benchmark.sh {benchmarkCorrelationId}");

                // Send files content as log messages
                foreach (var file in Directory.EnumerateFiles(logFolderPath, "*.log", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        _logger.LogInformation(
                            "[Benchmark {CorrelationId}] Log file {LogFileName}: {LogFileContent}",
                            benchmarkCorrelationId, Path.GetFileName(file), File.ReadAllText(file));
                    }
                    catch (Exception e)
                    {
                        // Do not throw exception as there could be other log files
                        _logger.LogException(e, 
                            "[Benchmark {CorrelationId}] Can't read file: {LogFileName}", 
                            benchmarkCorrelationId, Path.GetFileName(file));
                    }
                }
                
                // Get content from the telemetry report file
                string telemetryReportPath = $"{logFolderPath}/telemetry.json";
                string telemetryReport = string.Empty;
                try
                {
                    telemetryReport = File.ReadAllText(telemetryReportPath);
                }
                catch (Exception e)
                {
                    _logger.LogException(e,
                        "[Benchmark {CorrelationId}] Can't read telemetry report file: {LogFileName}", 
                        benchmarkCorrelationId, telemetryReportPath);
                    
                    Console.WriteLine(e);
                }
                
                result = new HPCBenchmarkResult(benchmarkCorrelationId, bashResult, telemetryReport);
            }
            catch (Exception e)
            {
                _logger.LogException(e, 
                    "[Benchmark {CorrelationId}] Error while running benchmark", 
                    benchmarkCorrelationId);
                
                Console.WriteLine(e);
                throw;
            }

            return await Task.FromResult(builder.Build(result.HappinessGrade, result));
        }

        private static BashResult Bash(string arguments)
        {
            if (string.IsNullOrEmpty(arguments))
                throw new ArgumentNullException(nameof(arguments));

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                var outputDataString = new StringBuilder();
                var errorDataString = new StringBuilder();

                process.OutputDataReceived += (s, e) =>
                {
                    outputDataString.AppendLine($"{e?.Data}");
                    
                    // Push process logs to stdout (useful for docker and kubernetes deployments)
                    Console.WriteLine(e?.Data);
                };
                process.ErrorDataReceived += (s, e) => outputDataString.AppendLine($"{e?.Data}");

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            
                return new BashResult
                {
                    Stdout = outputDataString.ToString(),
                    Stderr = errorDataString.ToString(),
                    ExitCode = process.ExitCode
                };
            }
            catch (Exception e)
            {
                return new BashResult
                {
                    Stderr = e.ToString()
                };
            }
        }
    }
}
