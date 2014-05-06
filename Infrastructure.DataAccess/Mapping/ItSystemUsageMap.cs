using Core.DomainModel.ItSystem;
using System.Data.Entity.ModelConfiguration;

namespace Infrastructure.DataAccess.Mapping
{
    class ItSystemUsageMap : EntityTypeConfiguration<ItSystemUsage>
    {
        public ItSystemUsageMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ItSystemUsage");
            this.Property(t => t.Id)
                .HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.Organization)
                .WithMany(t => t.ItSystemUsages);

            this.HasRequired(t => t.ObjectOwner)
                .WithMany(user => user.CreatedSystemUsages)
                .HasForeignKey(t => t.ObjectOwnerId);

            this.HasMany(t => t.OrgUnits)
                 .WithMany(t => t.ItSystemUsages)
                 .Map(t => t.ToTable("OrgUnitSystemUsage"));

            this.HasOptional(t => t.ResponsibleUnit)
                 .WithMany(t => t.DelegatedSystemUsages);

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
                .WithMany(t => t.Using)
                .Map(mc =>
                    {
                        mc.MapLeftKey("UsageId");
                        mc.MapRightKey("OrgUnitId");
                    });
        }
    }
}
