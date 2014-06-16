using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemMap : EntityMap<ItSystem>
    {
        public ItSystemMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("ItSystem");

            // Relationships
            this.HasOptional(t => t.Parent)
                .WithMany(d => d.Children)
                .HasForeignKey(t => t.ParentId)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.Organization)
                .WithMany(d => d.ItSystems)
                .HasForeignKey(t => t.OrganizationId);

            this.HasOptional(t => t.BelongsTo)
                .WithMany(d => d.BelongingSystems)
                .HasForeignKey(t => t.BelongsToId);

            this.HasOptional(t => t.AppType)
                .WithMany(t => t.References)
                .HasForeignKey(t => t.AppTypeId);

            this.HasOptional(t => t.BusinessType)
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
