using System;
using System.Threading.Tasks;
using RoboCustos.Brain;

namespace LaNina.Robot
{
    class ConsoleLocker : RoboStorageLocker
    {
        public override async Task StoreResult(string robotName, InteractionInformation interactionInformation, Interaction result)
        {
            Console.WriteLine($"{DateTime.Now}: {robotName} - {result.HappinessGrade} - {result.HappinessExplanation}");
        }
    }
}