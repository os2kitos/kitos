using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystemUsage;

namespace Infrastructure.DataAccess.Mapping
{
    class ItInterfaceExhibitUsageMap : EntityTypeConfiguration<ItInterfaceExhibitUsage>
    {
        public ItInterfaceExhibitUsageMap()
        {
            // Primary key
            this.HasKey(x => new { x.ItSystemUsageId, x.ItInterfaceExhibitId });

            // Table & Column Mappings
            this.ToTable("ItInterfaceExhibitUsage");

            this.HasRequired(t => t.ItSystemUsage)
                .WithMany(d => d.ItInterfaceExhibitUsages)
                .HasForeignKey(t => t.ItSystemUsageId)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.ItInterfaceExhibit)
                .WithMany(d => d.ItInterfaceExhibitUsage)
                .HasForeignKey(t => t.ItInterfaceExhibitId);

            this.HasOptional(t => t.ItContract)
                .WithMany(d => d.AssociatedInterfaceExposures)
                .HasForeignKey(t => t.ItContractId);
        }
    }
}
