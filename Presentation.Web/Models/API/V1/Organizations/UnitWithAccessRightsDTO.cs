namespace Presentation.Web.Models.API.V1.Organizations
{
    public class UnitWithAccessRightsDTO : UnitAccessRightsDTO
    {
        public int UnitId { get; set; }

        public UnitWithAccessRightsDTO(int unitId, bool canBeRead, bool canBeModified, bool canNameBeModified, bool canEanBeModified, bool canDeviceIdBeModified, bool canBeRearranged, bool canBeDeleted) 
            : base(canBeRead, canBeModified, canNameBeModified, canEanBeModified, canDeviceIdBeModified, canBeRearranged, canBeDeleted)
        {
            UnitId = unitId;
        }
    }
}