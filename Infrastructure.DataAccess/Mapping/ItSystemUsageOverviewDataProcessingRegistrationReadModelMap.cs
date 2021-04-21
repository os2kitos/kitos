using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage.Read;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewDataProcessingRegistrationReadModelMap : EntityTypeConfiguration<ItSystemUsageOverviewDataProcessingRegistrationReadModel>
    {
        public ItSystemUsageOverviewDataProcessingRegistrationReadModelMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.DataProcessingRegistrations)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(true);

            Property(x => x.DataProcessingRegistrationId)
                .IsRequired()
                .HasIndexAnnotation("ItSystemUsageOverviewArchivePeriodReadModel_index_DataProcessingRegistrationId", 0);

            Property(x => x.DataProcessingRegistrationName)
                .IsRequired()
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewArchivePeriodReadModel_index_DataProcessingRegistrationName", 0);

            Property(x => x.IsAgreementConcluded)
                .IsOptional();
        }
    }
}
