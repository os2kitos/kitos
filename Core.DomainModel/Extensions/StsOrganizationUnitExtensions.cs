using System.Linq;
using Core.DomainModel.Organization;

namespace Core.DomainModel.Extensions
{
    public static class StsOrganizationUnitExtensions
    {
        public static OrganizationUnit ToOrganizationUnit(this ExternalOrganizationUnit stsOrganizationUnit, OrganizationUnitOrigin origin, Organization.Organization parentOrganization, bool includeChildren = true)
        {
            var organizationUnit = new OrganizationUnit
            {
                Name = stsOrganizationUnit.Name.Length > OrganizationUnit.MaxNameLength ? stsOrganizationUnit.Name.Substring(0, OrganizationUnit.MaxNameLength) : stsOrganizationUnit.Name,
                Origin = origin,
                ExternalOriginUuid = stsOrganizationUnit.Uuid,
                Organization = parentOrganization
            };
            if (includeChildren)
            {
                organizationUnit.Children = stsOrganizationUnit
                    .Children
                    .Select(child => child.ToOrganizationUnit(origin, parentOrganization))
                    .ToList();
            }
            return organizationUnit;
        }
    }
}
