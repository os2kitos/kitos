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
            this.HasOptional(t => t.Parent)
                .WithMany(d => d.Children)
                .HasForeignKey(t => t.ParentId)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.Organization)
                .WithMany(d => d.ItSystems)
                .HasForeignKey(t => t.OrganizationId);

            this.HasRequired(t => t.BelongsTo)
                .WithMany(d => d.BelongingSystems)
                .HasForeignKey(t => t.BelongsToId);

            this.HasRequired(t => t.User)
                .WithMany(user => user.CreatedSystems)
                .HasForeignKey(t => t.UserId);

            this.HasRequired(t => t.AppType)
                .WithMany(t => t.References)
                .HasForeignKey(t => t.AppTypeId);

            this.HasRequired(t => t.BusinessType)
                .WithMany(t => t.References)
                .HasForeignKey(t => t.BusinessTypeId);

            this.HasOptional(t => t.Interface)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.InterfaceId);

            this.HasOptional(t => t.InterfaceType)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.InterfaceTypeId);

            this.HasOptional(t => t.Method)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.MethodId);
            
            this.HasMany(t => t.CanUseInterfaces)
                .WithMany(d => d.CanBeUsedBy);

            this.HasOptional(t => t.ExposedBy)
                .WithMany(d => d.ExposedInterfaces)
                .HasForeignKey(d => d.ExposedById)
                .WillCascadeOnDelete(false);
        }
    }
}
