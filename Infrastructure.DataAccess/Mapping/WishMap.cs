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

            this.HasRequired(t => t.User)
                .WithMany(t => t.Wishes)
                .HasForeignKey(d => d.UserId);
            

            this.HasRequired(t => t.ItSystemUsage)
                .WithMany(t => t.Wishes)
                .HasForeignKey(d => d.ItSystemUsageId);

        }
    }
}
