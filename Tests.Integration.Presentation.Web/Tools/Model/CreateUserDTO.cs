
namespace Tests.Integration.Presentation.Web.Tools.Model
{
    public class CreateUserDTO
    {
        public ApiUserDTO user { get; set; }

        public int organizationId { get; set; }

        public bool sendMailOnCreation { get; set; }
    }

    public class ApiUserDTO
    {
        public string Name { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public bool? HasApiAccess{ get; set; }
    }
}
