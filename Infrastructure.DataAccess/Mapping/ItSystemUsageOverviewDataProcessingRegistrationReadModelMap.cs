using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewDataProcessingRegistrationReadModelMap : IEntityTypeConfiguration<ItSystemUsageOverviewDataProcessingRegistrationReadModel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOverviewDataProcessingRegistrationReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.DataProcessingRegistrations)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.DataProcessingRegistrationId).IsRequired();
            builder.HasIndex(x => x.DataProcessingRegistrationId).HasDatabaseName("ItSystemUsageOverviewArchivePeriodReadModel_index_DataProcessingRegistrationId");

            builder.Property(x => x.DataProcessingRegistrationUuid).IsRequired();

            builder.Property(x => x.DataProcessingRegistrationName).IsRequired().HasMaxLength(DataProcessingRegistrationConstraints.MaxNameLength);
            builder.HasIndex(x => x.DataProcessingRegistrationName).HasDatabaseName("ItSystemUsageOverviewArchivePeriodReadModel_index_DataProcessingRegistrationName");
        }
    }
}
