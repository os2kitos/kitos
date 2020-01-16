namespace Core.DomainModel.KLE
{
    public class KLEUpdateHistoryItem: Entity
    {
        public string Version { get; set; }

        public KLEUpdateHistoryItem(string version, int userId)
        {
            Version = version;
            LastChangedByUserId = userId;
        }
    }
}