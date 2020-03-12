using System;

namespace Core.DomainModel.ItSystemUsage.GDPR
{
    public class ItSystemUsageSensitiveDataLevel
    {
        public int Id { get; set; }
        public virtual ItSystemUsage ItSystemUsage { get; set; }
        public SensitiveDataLevel SensitiveDataLevel { get; set; }
    }
}
