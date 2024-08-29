using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit
{
    public class UnitAccessRightsWithUnitDataResponseDTO
    {
        public UnitAccessRightsWithUnitDataResponseDTO(UnitAccessRights unitAccessRights, OrganizationUnitResponseDTO organizationUnit)
        {
            UnitAccessRights = new UnitAccessRightsResponseDTO(unitAccessRights);
            OrganizationUnit = organizationUnit;
        }

        public UnitAccessRightsWithUnitDataResponseDTO(){}

        public OrganizationUnitResponseDTO OrganizationUnit { get; set; }
        public UnitAccessRightsResponseDTO UnitAccessRights { get; set; }
    }
}