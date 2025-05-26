using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationRightMap : EntityMap<OrganizationRight>
    {
        public OrganizationRightMap()
        {
            this.HasRequired(right => right.Organization)
                .WithMany(obj => obj.Rights)
                .HasForeignKey(right => right.OrganizationId);

            this.HasRequired(t => t.User)
                .WithMany(d => d.OrganizationRights)
                .HasForeignKey(t => t.UserId);

            this.HasOptional(t => t.DefaultOrgUnit)
                .WithMany(d => d.DefaultUsers)
                .HasForeignKey(t => t.DefaultOrgUnitId);
        }
    }
}
