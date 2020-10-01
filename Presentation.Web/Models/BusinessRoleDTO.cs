namespace Presentation.Web.Models
{
    public class BusinessRoleDTO : NamedEntityDTO
    {
        public bool HasWriteAccess { get; set; }
        
        public string Note { get; set; }

        public BusinessRoleDTO(int id, string name) 
            : base(id, name)
        {
        }
    }
}