using System.Linq;
using Core.DomainModel.Organization;

namespace Core.DomainModel.Extensions
{
    public static class StsOrganizationUnitExtensions
    {
        public static OrganizationUnit ToOrganizationUnit(this ExternalOrganizationUnit stsOrganizationUnit, OrganizationUnitOrigin origin, Organization.Organization parentOrganization)
        {
            return new OrganizationUnit
            {
                Name = stsOrganizationUnit.Name,
                Origin = origin,
                ExternalOriginUuid = stsOrganizationUnit.Uuid,
                Organization = parentOrganization,
                Children = stsOrganizationUnit
                    .Children
                    .Select(child => child.ToOrganizationUnit(origin, parentOrganization))
                    .ToList()
            };
        }
    }
}
