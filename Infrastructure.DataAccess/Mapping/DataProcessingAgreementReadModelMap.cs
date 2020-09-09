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