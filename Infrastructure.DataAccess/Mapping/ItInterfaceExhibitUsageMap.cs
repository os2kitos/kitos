using Core.DomainModel.ItSystemUsage;

namespace Infrastructure.DataAccess.Mapping
{
    class ItInterfaceExhibitUsageMap : EntityMap<ItInterfaceExhibitUsage>
    {
        public ItInterfaceExhibitUsageMap()
        {
            // Table & Column Mappings
            this.ToTable("ItInterfaceExhibitUsage");

            this.HasRequired(t => t.ItSystemUsage)
                .WithMany(d => d.ItInterfaceExhibitUsages)
                .HasForeignKey(t => t.ItSystemUsageId);

            this.HasRequired(t => t.ItInterfaceExhibit)
                .WithMany(d => d.ItInterfaceExhibitUsage)
                .HasForeignKey(t => t.ItInterfaceExhibitId);

            this.HasOptional(t => t.ItContract)
                .WithMany(d => d.AssociatedInterfaceExposures)
                .HasForeignKey(t => t.ItContractId);
        }
    }
}
