namespace Presentation.Web.Models.API.V1.Organizations
{
    public class UnitAccessRightsDTO
    {
        public UnitAccessRightsDTO(bool canBeRead, bool canBeModified, bool canNameBeModified, bool canBeRearranged, bool canBeDeleted)
        {
            CanBeRead = canBeRead;
            CanBeModified = canBeModified;
            CanNameBeModified = canNameBeModified;
            CanBeRearranged = canBeRearranged;
            CanBeDeleted = canBeDeleted;
        }

        public bool CanBeRead { get; set; }
        public bool CanBeModified { get; set; }
        public bool CanNameBeModified { get; set; }
        public bool CanBeRearranged { get; set; }
        public bool CanBeDeleted { get; set; }
    }
}