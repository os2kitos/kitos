using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingAgreementReadModelMap : EntityTypeConfiguration<DataProcessingAgreementReadModel>
    {
        public DataProcessingAgreementReadModelMap()
        {
            Property(x => x.Name)
                .HasMaxLength(DataProcessingAgreementConstraints.MaxNameLength)
                .IsRequired()
                .HasIndexAnnotation("DataProcessingAgreementReadModel_Index_Name", 0);

            Property(x => x.MainReferenceTitle)
                .HasMaxLength(DataProcessingAgreementConstraints.MaxNameLength)
                .IsOptional()
                .HasIndexAnnotation("DataProcessingAgreementReadModel_Index_MainReferenceTitle", 0);

            //No index of this, length is unknown since no bounds on system assignment.
            Property(x => x.SystemNamesAsCsv).IsOptional();

            Property(x => x.MainReferenceUserAssignedId).IsOptional();

            Property(x => x.MainReferenceUrl).IsOptional();

            HasRequired(t => t.Organization)
                .WithMany(t => t.DataProcessingAgreementReadModels)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.SourceEntity)
                .WithMany(x => x.ReadModels)
                .HasForeignKey(x => x.SourceEntityId)
                .WillCascadeOnDelete(false);
        }
    }
}