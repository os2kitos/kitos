namespace Presentation.Web.Models.API.V1
{
    public class AssignedRoleDTO
    {
        public UserWithEmailDTO User { get; set; }
        public BusinessRoleDTO Role { get; set; }
    }
}