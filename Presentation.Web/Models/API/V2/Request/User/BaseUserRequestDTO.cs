namespace Presentation.Web.Models.API.V2.Request.User
{
    public class BaseUserRequestDTO
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DefaultUserStartPreferenceChoice DefaultUserStartPreference{ get; set; }
        public bool HasApiAccess { get; set; }
        public bool HasStakeHolderAccess { get; set; }
    }
}