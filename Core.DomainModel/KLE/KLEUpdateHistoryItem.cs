using System;

namespace Core.DomainModel.KLE
{
    public class KLEUpdateHistoryItem: Entity
    {
        public DateTime Version { get; set; }

        // Empty constructor to keep EF and proxy generation happy
        protected KLEUpdateHistoryItem() {}

        public KLEUpdateHistoryItem(DateTime version)
        {
            Version = version;
        }
    }
}