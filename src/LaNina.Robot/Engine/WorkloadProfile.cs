using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace LaNina.Robot.Engine
{
    internal class WorkloadProfile
    {
        public string Scheduler { get; }
        public Dictionary<LaNinaPersona, double> InteractionRates { get; }

        public WorkloadProfile(IConfigurationSection workloadProfileSection)
        {
            Scheduler = workloadProfileSection.GetValue<string>("Scheduler");
            InteractionRates = new Dictionary<LaNinaPersona, double>();
            var rates = workloadProfileSection.GetSection("InteractionRates");
            foreach (var rate in rates.GetChildren())
            {
                InteractionRates.Add(Enum.Parse<LaNinaPersona>(rate.Key), double.Parse(rate.Value));
            }
        }
    }
}