using System;
using System.Collections.Generic;
using System.Linq;
using LaNina.Robot.Engine.Benchmarks.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoboCustos.Brain;
using RoboCustos.Brain.InteractionsCore;

namespace LaNina.Robot.Engine.Benchmarks
{
    // ReSharper disable once InconsistentNaming
    internal class HPCBenchmarkResult: IStorable
    {
        private readonly BashResult _bashResult;
        
        public Guid CorrelationId { get; set; }
        public Telemetry Telemetry { get; set; }
        
        #region Constructors

        public HPCBenchmarkResult(Guid becnhmarkCorrelationId, BashResult bashResult, string telemetryReportString)
        {
            CorrelationId = becnhmarkCorrelationId;
            _bashResult = bashResult;

            if (!string.IsNullOrEmpty(telemetryReportString))
            {
                Telemetry = JsonConvert.DeserializeObject<Telemetry>(telemetryReportString);
            }
        }

        #endregion

        internal ExplainedInteractionHappinessGrade HappinessGrade
        {
            get
            {
                if (string.IsNullOrEmpty(_bashResult.Stdout) || !string.IsNullOrEmpty(_bashResult.Stderr))
                {
                    return ExplainedInteractionHappinessGrade.Unacceptable($"Error executing bash script");
                }

                var linpackHappinessGrade = ValidateLinpack(Telemetry);
                var streamHappinessGrade = ValidateStream(Telemetry);
                var pingPongHappinessGrade = ValidatePingPong(Telemetry);
                var openfoamHappinessGrade = ValidateOpenfoam(Telemetry);

                if (linpackHappinessGrade.InteractionHappinessGrade != InteractionHappinessGrade.Perfect)
                {
                    return linpackHappinessGrade;
                }
                if (streamHappinessGrade.InteractionHappinessGrade != InteractionHappinessGrade.Perfect)
                {
                    return streamHappinessGrade;
                }
                if (pingPongHappinessGrade.InteractionHappinessGrade != InteractionHappinessGrade.Perfect)
                {
                    return pingPongHappinessGrade;
                }
                if (openfoamHappinessGrade.InteractionHappinessGrade != InteractionHappinessGrade.Perfect)
                {
                    return openfoamHappinessGrade;
                }
                
                return ExplainedInteractionHappinessGrade.Perfect();
            }
        }

        public JContainer ToJson(StorageVerbosity verbosity)
        {
            return new JObject(
                new JProperty("CorrelationId", CorrelationId),
                
                new JProperty("telemetry", JObject.FromObject(Telemetry)),
                
                new JProperty("linpack", new JObject(
                    new JProperty("gflops_min", Telemetry.SingleHplTelemetry.Results.Min(r => r.Gflops)),
                    new JProperty("gflops_max", Telemetry.SingleHplTelemetry.Results.Max(r => r.Gflops)),
                    new JProperty("gflops_avg", Telemetry.SingleHplTelemetry.Results.Average(r => r.Gflops)),
                    new JProperty("number_of_nodes_below_threshold", Telemetry.SingleHplTelemetry.Results.Count(r => r.Gflops <= 630)),
                    new JProperty("number_of_nodes_above_threshold", Telemetry.SingleHplTelemetry.Results.Count(r => r.Gflops > 630))
                    )
                ),
                new JProperty("stream", new JObject(
                    new JProperty("triad_min", Telemetry.StreamTelemetry.Results.Min(r => r.Triad)),
                    new JProperty("triad_max", Telemetry.StreamTelemetry.Results.Max(r => r.Triad)),
                    new JProperty("triad_avg", Telemetry.StreamTelemetry.Results.Average(r => r.Triad)),
                    new JProperty("number_of_nodes_below_threshold", Telemetry.StreamTelemetry.Results.Count(r => r.Triad <= (decimal)70000)),
                    new JProperty("number_of_nodes_above_threshold", Telemetry.StreamTelemetry.Results.Count(r => r.Triad > (decimal)70000))
                    )
                )
            );
        }

        #region Validation

        private static ExplainedInteractionHappinessGrade ValidateLinpack(Telemetry telemetry)
        {
            const int threshold = 630;
            
            if (telemetry?.SingleHplTelemetry?.Results == null)
            {
                return ExplainedInteractionHappinessGrade.Unacceptable("No telemetry available");
            }

            var nodesBelowThreshold = telemetry.SingleHplTelemetry.Results.Where(r => r.Gflops < threshold).ToList();
            if (nodesBelowThreshold.Any())
            {
                return ExplainedInteractionHappinessGrade.Unacceptable(
                    $"GFlops value from nodes {string.Join(", ", nodesBelowThreshold)} was below the threshold: {threshold}" );
            }
            
            return ExplainedInteractionHappinessGrade.Perfect();
        }
        
        private static ExplainedInteractionHappinessGrade ValidateStream(Telemetry telemetry)
        {
            const int threshold = 70000;
            
            if (telemetry?.StreamTelemetry?.Results == null)
            {
                return ExplainedInteractionHappinessGrade.Unacceptable("No telemetry available");
            }

            var nodesBelowThreshold = telemetry.StreamTelemetry.Results.Where(r => r.Triad < threshold).ToList();
            if (nodesBelowThreshold.Any())
            {
                return ExplainedInteractionHappinessGrade.Unacceptable(
                    $"Triad value from nodes {string.Join(", ", nodesBelowThreshold)} was below the threshold: {threshold}" );
            }
            
            return ExplainedInteractionHappinessGrade.Perfect();
        }
        
        private static ExplainedInteractionHappinessGrade ValidatePingPong(Telemetry telemetry)
        {
            return ExplainedInteractionHappinessGrade.Perfect();
        }
        
        private static ExplainedInteractionHappinessGrade ValidateOpenfoam(Telemetry telemetry)
        {
            if (telemetry?.BenchmarkTelemetry?.OpenfoamTelemetry?.Results == null)
            {
                return ExplainedInteractionHappinessGrade.Unacceptable("No telemetry available");
            }
            
            var thresholds = new Dictionary<int, decimal>
            {
                {128, new decimal(8.151072)},
                {256, new decimal(8.151072)},
                {512, new decimal(8.151072)},
                {1024, new decimal(8.151072)}
            };

            var nodesBelowThreshold = telemetry.BenchmarkTelemetry.OpenfoamTelemetry.Results
                .Where(r => thresholds.ContainsKey(r.Workers) && r.AvgElapsedTimeInSec >= thresholds[r.Workers])
                .ToList();
            if (nodesBelowThreshold.Any())
            {
                return ExplainedInteractionHappinessGrade.Unacceptable(
                    $"OpenFoam results for the cores {string.Join(", ", nodesBelowThreshold.Select(n => n.Workers))} were below allowed thresholds" );
            }
            
            return ExplainedInteractionHappinessGrade.Perfect();
        }

        #endregion
    }
}