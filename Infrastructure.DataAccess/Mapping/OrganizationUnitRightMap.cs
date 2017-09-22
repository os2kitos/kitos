using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationUnitRightMap : RightMap<OrganizationUnit, OrganizationUnitRight, OrganizationUnitRole>
    {
        public OrganizationUnitRightMap()
        {
            this.HasRequired(x => x.User)
                .WithMany(x => x.OrganizationUnitRights)
                .HasForeignKey(x => x.UserId);
        }
    }
}
