namespace Presentation.Web.Models.API.V1.Users
{
    public class UserWithApiAccessDTO : UserWithEmailDTO
    {
        public bool ApiAccess { get; set; }

        public UserWithApiAccessDTO(int id, string fullName, string email, bool apiAccess) : base(id, fullName, email)
        {
            ApiAccess = apiAccess;
        }
    }
}