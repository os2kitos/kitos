using System;

namespace Core.DomainModel.KLE
{
    public class KLEUpdateHistoryItem: Entity
    {
        public DateTime Version { get; set; }

        private KLEUpdateHistoryItem()
        {
            //NOTE: For EF
        }

        public KLEUpdateHistoryItem(DateTime version, int userId)
        {
            Version = version;
            LastChangedByUserId = userId;
        }
    }
}