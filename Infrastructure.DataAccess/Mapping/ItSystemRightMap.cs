using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemRightMap : RightMap<ItSystemUsage, ItSystemRight, ItSystemRole>
    {
        public ItSystemRightMap()
        {
            this.HasRequired(x => x.User)
                .WithMany(x => x.ItSystemRights)
                .HasForeignKey(x => x.UserId);
        }
    }
}
