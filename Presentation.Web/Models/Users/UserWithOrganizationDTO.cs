namespace Presentation.Web.Models.Users
{
    public class UserWithOrganizationDTO : UserWithApiAccessDTO
    {
        public string OrgName { get; set; }
        public UserWithOrganizationDTO(int id, string fullName, string email, string orgName, bool apiAccess) 
            : base(id, fullName, email, apiAccess)
        {
            OrgName = orgName;
        }
    }
}
