using Core.DomainModel.ItSystemUsage.GDPR;
using System.Data.Entity.ModelConfiguration;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsagePersonalDataOptionsMap : EntityTypeConfiguration<ItSystemUsagePersonalData>
    {
        public ItSystemUsagePersonalDataOptionsMap()
        {
            Property(x => x.PersonalData)
                .IsRequired();
        }
    }
}
