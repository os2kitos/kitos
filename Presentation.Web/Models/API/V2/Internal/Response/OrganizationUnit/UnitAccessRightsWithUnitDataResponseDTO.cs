using System.Diagnostics.CodeAnalysis;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit
{
    public class UnitAccessRightsWithUnitDataResponseDTO
    {
        [SetsRequiredMembers]
        public UnitAccessRightsWithUnitDataResponseDTO(UnitAccessRights unitAccessRights, OrganizationUnitResponseDTO organizationUnit)
        {
            UnitAccessRights = new UnitAccessRightsResponseDTO(unitAccessRights);
            OrganizationUnit = organizationUnit;
        }

        public UnitAccessRightsWithUnitDataResponseDTO(){}

        public required OrganizationUnitResponseDTO OrganizationUnit { get; set; }
        public required UnitAccessRightsResponseDTO UnitAccessRights { get; set; }
    }
}