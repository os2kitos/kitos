using System;

namespace Core.DomainModel.KLE
{
    public class KLEUpdateHistoryItem: Entity
    {
        public DateTime Version { get; set; }

        // To keep EF happy
        private KLEUpdateHistoryItem() {}

        public KLEUpdateHistoryItem(DateTime version, int userId)
        {
            Version = version;
            LastChangedByUserId = userId;
        }
    }
}