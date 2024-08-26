using Core.DomainModel.Organization;

namespace Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit
{
    public class UnitAccessRightsResponseDTO
    {
        public UnitAccessRightsResponseDTO(UnitAccessRights accessRights)
        {
            CanBeRead = accessRights.CanBeRead;
            CanBeModified = accessRights.CanBeModified;
            CanBeRenamed = accessRights.CanBeRenamed;
            CanEanBeModified = accessRights.CanEanBeModified;
            CanDeviceIdBeModified = accessRights.CanDeviceIdBeModified;
            CanBeRearranged = accessRights.CanBeRearranged;
            CanBeDeleted = accessRights.CanBeDeleted;
            CanEditRegistrations = accessRights.CanEditRegistrations;
        }

        public UnitAccessRightsResponseDTO(){}

        public bool CanBeRead { get; set; }
        public bool CanBeModified { get; set; }
        public bool CanBeRenamed { get; set; }
        public bool CanEanBeModified { get; set; }
        public bool CanDeviceIdBeModified { get; set; }
        public bool CanBeRearranged { get; set; }
        public bool CanBeDeleted { get; set; }
        public bool CanEditRegistrations { get; set; }
    }
}