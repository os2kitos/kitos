namespace Presentation.Web.Models.API.V1
{
    public class AssignedRoleDTO
    {
        public required UserWithEmailDTO User { get; set; }
        public required BusinessRoleDTO Role { get; set; }
    }
}