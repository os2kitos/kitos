namespace Presentation.Web.Models.Users
{
    public class UserWithOrganizationDTO : UserWithEmailDTO
    {
        public string OrgName { get; set; }
        public UserWithOrganizationDTO(int id, string fullName, string email, string orgName) 
            : base(id, fullName, email)
        {
            OrgName = orgName;
        }
    }
}
