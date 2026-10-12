using Core.DomainModel.ItSystemUsage.GDPR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsagePersonalDataOptionsMap : IEntityTypeConfiguration<ItSystemUsagePersonalData>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsagePersonalData> builder)
        {
            builder.ToTable("ItSystemUsagePersonalDatas");
            builder.Property(x => x.PersonalData).IsRequired();
        }
    }
}
