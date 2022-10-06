using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;

namespace Core.DomainServices.Extensions
{
    public static class StsOrganizationUnitExtensions
    {
        public static OrganizationUnit ToOrganizationUnit(this StsOrganizationUnit stsOrganizationUnit, Organization parentOrganization)
        {
            return new OrganizationUnit
            {
                Name = stsOrganizationUnit.Name,
                Origin = OrganizationUnitOrigin.STS_Organisation,
                ExternalOriginUuid = stsOrganizationUnit.Uuid,
                Organization = parentOrganization,
                Children = stsOrganizationUnit
                    .Children
                    .Select(child => child.ToOrganizationUnit(parentOrganization))
                    .ToList()
            };
        }
    }
}
