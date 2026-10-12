using Core.DomainModel.ItSystemUsage.GDPR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageSensitiveDataLevelMap : IEntityTypeConfiguration<ItSystemUsageSensitiveDataLevel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageSensitiveDataLevel> builder)
        {
            // ClientCascade: SQL Server ON DELETE NO ACTION (avoids multiple cascade paths),
            // but EF Core handles deletes in memory when the relationship is severed.
            builder
                .HasOne(x => x.ItSystemUsage)
                .WithMany(x => x.SensitiveDataLevels)
                .HasForeignKey("ItSystemUsage_Id")
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
