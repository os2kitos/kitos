using Core.DomainModel.ItSystemUsage;

namespace Infrastructure.DataAccess.Mapping
{
    class InterfaceUsageMap : EntityMap<InterfaceUsage>
    {
        public InterfaceUsageMap()
        {
            // Table & Column Mappings
            this.ToTable("InterfaceUsage");

            this.HasRequired(t => t.ItSystemUsage)
                .WithMany(d => d.InterfaceUsages)
                .HasForeignKey(t => t.ItSystemUsageId)
                .WillCascadeOnDelete(true);

            this.HasRequired(t => t.ItInterfaceExhibit)
                .WithMany(d => d.InterfaceLocalUsages)
                .HasForeignKey(t => t.ItInterfaceExhibitId)
                .WillCascadeOnDelete(true);

            this.HasOptional(t => t.ItContract)
                .WithMany(d => d.AssociatedInterfaceUsages)
                .HasForeignKey(t => t.ItContractId)
                .WillCascadeOnDelete(true);

            this.HasOptional(t => t.Infrastructure)
                .WithMany(d => d.InfrastructureUsage)
                .HasForeignKey(t => t.InfrastructureId);

        }
    }
}
