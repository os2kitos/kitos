using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Triggers
{
    public class AdviceTrigger
    {
        public Maybe<int> PartitionId { get; }
        public string Cron { get; }

        public AdviceTrigger(string cron, Maybe<int> partitionId)
        {
            Cron = cron;
            PartitionId = partitionId;
        }
    }
}
