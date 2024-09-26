using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Request.User
{
    public class BaseUserRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DefaultUserStartPreferenceChoice DefaultUserStartPreference{ get; set; }
        public bool HasApiAccess { get; set; }
        public bool HasStakeHolderAccess { get; set; }
    }
}