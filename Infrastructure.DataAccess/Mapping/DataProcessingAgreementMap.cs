using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.GDPR;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingAgreementMap : EntityTypeConfiguration<DataProcessingAgreement>
    {
        public DataProcessingAgreementMap()
        {
            Property(x => x.Name)
                .HasMaxLength(DataProcessingAgreementConstraints.MaxNameLength)
                .IsRequired()
                .HasIndexAnnotation("DataProcessingAgreement_Index_Name", 0);

            HasRequired(t => t.Organization)
                .WithMany(t => t.DataProcessingAgreements)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);
        }
    }
}