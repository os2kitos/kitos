using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class AdminRightMap : RightMap<Organization, AdminRight, AdminRole>
    {
        public AdminRightMap()
        {
            //This is already mapped in ctor of RightMap, but
            //because we need to specify the nav property on user,
            //we need to remap
            this.HasRequired(t => t.User)
                .WithMany(d => d.AdminRights)
                .HasForeignKey(t => t.UserId);
        }
    }
}