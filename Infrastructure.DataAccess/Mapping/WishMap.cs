using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class WishMap : EntityMap<Wish>
    {
        public WishMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("Wish");

            this.HasRequired(t => t.User)
                .WithMany(t => t.Wishes)
                .HasForeignKey(d => d.UserId);

            this.HasRequired(t => t.ItSystemUsage)
                .WithMany(t => t.Wishes)
                .HasForeignKey(d => d.ItSystemUsageId);
        }
    }
}
