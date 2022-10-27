namespace Presentation.Web.Models.API.V1.Organizations
{
    public class UnitAccessRightsDTO
    {
        public UnitAccessRightsDTO(bool canBeRead, bool canBeModified, bool canBeDeleted)
        {
            CanBeRead = canBeRead;
            CanBeModified = canBeModified;
            CanBeDeleted = canBeDeleted;
        }

        public bool CanBeRead { get; set; }
        public bool CanBeModified { get; set; }
        public bool CanBeDeleted { get; set; }
    }
}