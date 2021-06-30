namespace Presentation.Web.Models.Users
{
    public class UserWithCrossOrganizationalRightsDTO : UserWithEmailDTO
    {
        public bool ApiAccess { get; set; }
        public bool StakeholderAccess { get; set; }

        public UserWithCrossOrganizationalRightsDTO(int id, string fullName, string email, bool apiAccess, bool stakeholderAccess) : base(id, fullName, email)
        {
            ApiAccess = apiAccess;
            StakeholderAccess = stakeholderAccess;
        }
    }
}
