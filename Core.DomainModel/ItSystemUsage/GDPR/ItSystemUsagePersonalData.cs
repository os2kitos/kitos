namespace Core.DomainModel.ItSystemUsage.GDPR
{
    public class ItSystemUsagePersonalData
    {
        public int Id { get; set; }
        public virtual ItSystemUsage ItSystemUsage { get; set; }
        public GDPRPersonalDataOption PersonalData { get; set; }
    }
}
