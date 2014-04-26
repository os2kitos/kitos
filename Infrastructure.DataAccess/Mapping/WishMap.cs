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

            this.HasRequired(t => t.User); 
            //TODO .WithMany(d => d.Wishes)??

            this.HasRequired(t => t.ItSystem)
                .WithMany(d => d.Wishes);

        }
    }
}
