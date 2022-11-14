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

        public bool CanBeRead { get; }
        public bool CanBeModified { get; }
        public bool CanNameBeModified { get; }
        public bool CanEanBeModified { get; }
        public bool CanDeviceIdBeModified { get; }
        public bool CanBeRearranged { get; }
        public bool CanBeDeleted { get; }
    }
}