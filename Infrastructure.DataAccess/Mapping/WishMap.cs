using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class WishMap : EntityTypeConfiguration<Wish>
    {
        public WishMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Wish");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.FunctionalityId).HasColumnName("FunctionalityId");
            this.Property(t => t.InterfaceId).HasColumnName("InterfaceId");

            // Relationships
            this.HasOptional(t => t.Functionality)
                .WithMany(t => t.Wishes)
                .HasForeignKey(d => d.FunctionalityId);
            this.HasOptional(t => t.Interface)
                .WithMany(t => t.Wishes)
                .HasForeignKey(d => d.InterfaceId);

        }
    }
}
