using Core.DomainModel.GDPR;
using System.Data.Entity.ModelConfiguration;

namespace Infrastructure.DataAccess.Mapping
{
    public class SubDataProcessorMap : EntityTypeConfiguration<SubDataProcessor>
    {
        public SubDataProcessorMap()
        {
            HasKey(x => new { x.OrganizationId, x.DataProcessingRegistrationId });
            HasRequired(x => x.DataProcessingRegistration)
                .WithMany(x => x.AssignedSubDataProcessors)
                .HasForeignKey(x => x.DataProcessingRegistrationId)
                .WillCascadeOnDelete(true);

            HasRequired(x => x.Organization)
                .WithMany(x => x.SubDataProcessorRegistrations)
                .HasForeignKey(x => x.OrganizationId)
                .WillCascadeOnDelete(false);

            HasOptional(x => x.SubDataProcessorBasisForTransfer)
                .WithMany(x => x.SubDataProcessors)
                .HasForeignKey(x => x.SubDataProcessorBasisForTransferId)
                .WillCascadeOnDelete(false);

            HasOptional(x => x.InsecureCountry)
                .WithMany(x => x.SubDataProcessors)
                .HasForeignKey(x => x.InsecureCountryId)
                .WillCascadeOnDelete(false);
        }
    }
}
