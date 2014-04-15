using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationUnitMap : EntityTypeConfiguration<OrganizationUnit>
    {
        public OrganizationUnitMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("OrganizationUnit");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasOptional(o => o.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(o => o.ParentId)
                .WillCascadeOnDelete(true);

            this.HasRequired(o => o.Organization)
                .WithMany(m => m.OrgUnits)
                .HasForeignKey(o => o.OrganizationId)
                .WillCascadeOnDelete(true);
        }
    }
}
