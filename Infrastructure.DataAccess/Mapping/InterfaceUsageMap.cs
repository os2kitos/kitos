using Core.DomainModel.ItSystemUsage;

namespace Infrastructure.DataAccess.Mapping
{
    class InterfaceUsageMap : EntityMap<InterfaceUsage>
    {
        public InterfaceUsageMap()
        {
            // Table & Column Mappings
            this.ToTable("InfUsage");

            this.HasRequired(t => t.ItSystemUsage)
                .WithMany(d => d.InterfaceUsages)
                .HasForeignKey(t => t.ItSystemUsageId)
                .WillCascadeOnDelete(true);

            this.HasRequired(t => t.ItInterfaceUse)
                .WithMany(d => d.InterfaceUsages)
                .Map(m => m.MapKey(new[] { "infUseId", "infUsageId" })) // have to rename key else it's too long for MySql
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
