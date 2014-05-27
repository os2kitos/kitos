using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class EconomyStreamMap : EntityTypeConfiguration<EconomyStream>
    {
        public EconomyStreamMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("EconomyStream");
            this.Property(t => t.Id).HasColumnName("Id");

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
