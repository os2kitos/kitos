using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemMap : EntityTypeConfiguration<ItSystem>
    {
        public ItSystemMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ItSystem");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(d => d.ParentId);

            this.HasRequired(t => t.Organization)
                .WithMany(t => t.ItSystems)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.AppType)
                .WithMany(t => t.References)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.BusinessType)
                .WithMany(t => t.References)
                .WillCascadeOnDelete(false);

            this.HasMany(t => t.CanUseInterfaces)
                .WithMany(d => d.CanBeUsedBy);

            this.HasOptional(t => t.ExposedBy)
                .WithMany(d => d.ExposedInterfaces)
                .WillCascadeOnDelete(false);
        }
    }
}
