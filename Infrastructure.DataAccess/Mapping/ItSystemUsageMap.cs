using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    class ItSystemUsageMap : EntityMap<ItSystemUsage>
    {
        public ItSystemUsageMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("ItSystemUsage");

            // Relationships
            this.HasRequired(t => t.Organization)
                .WithMany(t => t.ItSystemUsages);

            this.HasMany(t => t.OrgUnits)
                .WithMany(t => t.ItSystemUsages)
                .Map(t => t.ToTable("OrgUnitSystemUsage"));

            this.HasOptional(t => t.ResponsibleUsage)
                .WithOptionalPrincipal();

            this.HasRequired(t => t.ItSystem)
                .WithMany(t => t.Usages);

            this.HasOptional(t => t.ArchiveType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ArchiveTypeId);

            this.HasOptional(t => t.SensitiveDataType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.SensitiveDataTypeId);

            this.HasOptional(t => t.OverviewItSystem)
                .WithMany(t => t.Overviews)
                .HasForeignKey(d => d.OverviewItSystemId)
                .WillCascadeOnDelete(false);

            this.HasMany(t => t.UsedBy)
                .WithRequired(t => t.ItSystemUsage)
                .HasForeignKey(d => d.ItSystemUsageId);

            this.HasOptional(t => t.MainContract)
                .WithMany()
                .HasForeignKey(t => t.MainContractId);
        }
    }
}
