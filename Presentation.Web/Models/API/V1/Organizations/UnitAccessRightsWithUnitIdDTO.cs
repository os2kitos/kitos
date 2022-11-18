namespace Presentation.Web.Models.API.V1.Organizations
{
    public class UnitAccessRightsWithUnitIdDTO : UnitAccessRightsDTO
    {
        public int UnitId { get; set; }

        public UnitAccessRightsWithUnitIdDTO(int unitId, UnitAccessRightsDTO rights) 
            : base(rights)
        {
            UnitId = unitId;
        }
    }
}