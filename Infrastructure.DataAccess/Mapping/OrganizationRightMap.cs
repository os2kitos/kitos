using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationRightMap : RightMap<Organization, OrganizationRight, OrganizationRole>
    {
        public OrganizationRightMap()
        {
            //This is already mapped in ctor of RightMap, but
            //because we need to specify the nav property on user,
            //we need to remap
            this.HasRequired(t => t.User)
                .WithMany(d => d.OrganizationRights)
                .HasForeignKey(t => t.UserId);

            this.HasOptional(t => t.DefaultOrgUnit)
                .WithMany(d => d.DefaultUsers)
                .HasForeignKey(t => t.DefaultOrgUnitId);
        }
    }
}
