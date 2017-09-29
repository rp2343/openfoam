using System;
using LaNina.Robot.Engine.Benchmarks;
using Microsoft.Extensions.Logging;
using RoboCustos.Brain;

namespace LaNina.Robot.Engine
{
    internal class LaNinaPersonaMother : RoboPersonaMother<LaNinaPersonaMother>
    {
        #region Constructor

        public LaNinaPersonaMother(Func<LaNinaPersonaMother, RoboPersona> robotFactory, double targetNumberOfInteractionsPerMinute, string name)
            : base(robotFactory, targetNumberOfInteractionsPerMinute, name)
        {
        }

        #endregion

        public static LaNinaPersonaMother MotherFor(LaNinaPersona persona, double targetNumberOfInteractionsPerMinute, ILogger logger)
        {
            return new LaNinaPersonaMother(FactoryFor(persona, logger), targetNumberOfInteractionsPerMinute, $"{nameof(LaNina)}.{persona}");
        }

        private static Func<LaNinaPersonaMother, RoboPersona> FactoryFor(LaNinaPersona persona, ILogger logger)
        {
            switch (persona)
            {
                // Customer Personas
                case LaNinaPersona.HPCBenchmark:
                    return personaMother => new HPCBenchmark(personaMother, logger);

                default:
                    throw new InvalidOperationException($"Unknown persona: {KnownProductNames.Rhapso}.{persona}");
            }
        }
    }
}