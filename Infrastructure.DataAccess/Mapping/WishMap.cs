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
            this.Property(t => t.Functionality_Id).HasColumnName("Functionality_Id");
            this.Property(t => t.Interface_Id).HasColumnName("Interface_Id");

            // Relationships
            this.HasOptional(t => t.Functionality)
                .WithMany(t => t.Wishes)
                .HasForeignKey(d => d.Functionality_Id);
            this.HasOptional(t => t.Interface)
                .WithMany(t => t.Wishes)
                .HasForeignKey(d => d.Interface_Id);

        }
    }
}
