using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.GDPR;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingAgreementMap : EntityTypeConfiguration<DataProcessingAgreement>
    {
        public DataProcessingAgreementMap()
        {
            //Simple properties
            Property(x => x.Name)
                .HasMaxLength(DataProcessingAgreementConstraints.MaxNameLength)
                .IsRequired()
                .HasIndexAnnotation("DataProcessingAgreement_Index_Name", 0);

            //Organization relationship
            HasRequired(t => t.Organization)
                .WithMany(t => t.DataProcessingAgreements)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);

            //External references
            HasOptional(t => t.Reference);
            HasMany(t => t.ExternalReferences)
                .WithOptional(d => d.DataProcessingAgreement)
                .HasForeignKey(d => d.DataProcessingAgreement_Id)
                .WillCascadeOnDelete(true);

            //It-systems
            HasMany(x=>x.SystemUsages)
                .WithMany(x=>x.AssociatedDataProcessingAgreements);
        }
    }
}