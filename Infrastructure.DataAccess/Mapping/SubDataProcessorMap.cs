using Core.DomainModel.GDPR;

namespace Infrastructure.DataAccess.Mapping
{
    public class SubDataProcessorMap : EntityMap<SubDataProcessor>
    {
        public SubDataProcessorMap()
        {
            HasRequired(x => x.Organization)
                .WithMany(x => x.SubDataProcessorRegistrations)
                .HasForeignKey(x=>x.OrganizationId)
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
