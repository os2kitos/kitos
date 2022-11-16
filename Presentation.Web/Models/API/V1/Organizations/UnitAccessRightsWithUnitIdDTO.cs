namespace Presentation.Web.Models.API.V1.Organizations
{
    public class UnitAccessRightsWithUnitIdDTO : UnitAccessRightsDTO
    {
        public int UnitId { get; set; }

        public UnitAccessRightsWithUnitIdDTO(int unitId, bool canBeRead, bool canBeModified, bool canNameBeModified, bool canEanBeModified, bool canDeviceIdBeModified, bool canBeRearranged, bool canBeDeleted) 
            : base(canBeRead, canBeModified, canNameBeModified, canEanBeModified, canDeviceIdBeModified, canBeRearranged, canBeDeleted)
        {
            UnitId = unitId;
        }
    }
}