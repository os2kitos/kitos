namespace Tests.Integration.Presentation.Web.Tools.Model
{
    public class CreateUserDTO
    {
        public ApiUserDTO user { get; set; }

        public int organizationId { get; set; }

        public bool sendMailOnCreation { get; set; }
    }
}
