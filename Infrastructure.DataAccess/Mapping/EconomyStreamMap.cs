using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class EconomyStreamMap : EntityMap<EconomyStream>
    {
        public EconomyStreamMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("EconomyStream");

            // Relationships
            this.HasOptional(t => t.ExternPaymentFor)
                .WithMany(d => d.ExternEconomyStreams)
                .HasForeignKey(t => t.ExternPaymentForId);

            this.HasOptional(t => t.InternPaymentFor)
                .WithMany(d => d.InternEconomyStreams)
                .HasForeignKey(t => t.InternPaymentForId);

            this.HasOptional(t => t.OrganizationUnit)
                .WithMany(d => d.EconomyStreams)
                .HasForeignKey(t => t.OrganizationUnitId);

        }
    }
}
