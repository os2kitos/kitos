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
        }
    }
}
