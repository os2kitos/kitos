namespace Presentation.Web.Models.API.V1.Organizations
{
    public class UnitAccessRightsDTO
    {
        public UnitAccessRightsDTO(bool canBeRead, bool canBeModified, bool canNameBeModified, bool canEanBeModified, bool canDeviceIdBeModified, bool canBeRearranged, bool canBeDeleted)
        {
            CanBeRead = canBeRead;
            CanBeModified = canBeModified;
            CanNameBeModified = canNameBeModified;
            CanEanBeModified = canEanBeModified;
            CanDeviceIdBeModified = canDeviceIdBeModified;
            CanBeRearranged = canBeRearranged;
            CanBeDeleted = canBeDeleted;
        }

        public bool CanBeRead { get; set; }
        public bool CanBeModified { get; set; }
        public bool CanNameBeModified { get; set; }
        public bool CanEanBeModified { get; }
        public bool CanDeviceIdBeModified { get; }
        public bool CanBeRearranged { get; set; }
        public bool CanBeDeleted { get; set; }
    }
}