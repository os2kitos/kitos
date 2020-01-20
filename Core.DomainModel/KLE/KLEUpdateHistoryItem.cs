using System;

namespace Core.DomainModel.KLE
{
    public class KLEUpdateHistoryItem: Entity
    {
        public DateTime Version { get; set; }

        public KLEUpdateHistoryItem()
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