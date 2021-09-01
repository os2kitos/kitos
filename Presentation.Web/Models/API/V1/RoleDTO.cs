namespace Presentation.Web.Models.API.V1
{
    public class RoleDTO : OptionDTO
    {
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
    }
}