namespace Tests.Integration.Presentation.Web.Tools.Model
{
    public class ApiUserDTO
    {
        public string Name { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public bool? HasApiAccess { get; set; }
        public bool? HasStakeHolderAccess { get; set; }
    }
}
