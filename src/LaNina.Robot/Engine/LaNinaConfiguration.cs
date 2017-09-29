using Microsoft.Extensions.Configuration;
using RoboCustos.Brain;

namespace LaNina.Robot.Engine
{
    class LaNinaConfiguration
    {
        private IConfigurationRoot ConfigurationRoot { get; }
        public WorkloadProfile WorkloadProfile { get; }

        public LaNinaConfiguration(IConfigurationRoot configurationRoot)
        {
            ConfigurationRoot = configurationRoot;
            WorkloadProfile = new WorkloadProfile(ConfigurationRoot.GetSection("Robot:WorkloadProfile"));
        }

        // Common settings
        public string PersonaResultsEventHubConnectionString => ConfigurationRoot.GetValue<string>("Robot:PersonaResultsEventHubConnectionString");

        public string TargetEnvironment => ConfigurationRoot.GetValue<string>("Robot:TargetEnvironment");
        public string LogEnabledLoggers => ConfigurationRoot.GetValue<string>("Robot:LogEnabledLoggers");
        public string LogEventHubConnectionString => ConfigurationRoot.GetValue<string>("Robot:LogEventHubConnectionString");
        public int? MaxDegreeOfParallelism => ConfigurationRoot.GetValue<int?>("Robot:MaxDegreeOfParallelism");
        public int? MaxMessagesPerTask => ConfigurationRoot.GetValue<int?>("Robot:MaxMessagesPerTask");
        public bool IncludeHappinessCounts => ConfigurationRoot.GetValue<bool>("Robot:IncludeHappinessCounts");
        public CloudPlatform CloudPlatform => CloudPlatform.Azure;
    }
}