namespace UI.MVC4.Models
{
    public class RoleDTO : OptionDTO
    {
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
    }
}