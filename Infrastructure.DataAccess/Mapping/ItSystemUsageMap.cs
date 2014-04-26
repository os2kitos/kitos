using Core.DomainModel.ItSystem;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
