namespace Presentation.Web.Models.API.V1
{
    public class BusinessRoleDTO : NamedEntityWithExpirationStatusDTO
    {
        public bool HasWriteAccess { get; set; }
        
        public string Note { get; set; }

        public BusinessRoleDTO(int id, string name, bool expired, bool hasWriteAccess, string note) 
            : base(id, name, expired)
        {
            HasWriteAccess = hasWriteAccess;
            Note = note;
        }
    }
}