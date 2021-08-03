namespace Presentation.Web.Models.API.V1
{
    public class UserWithEmailDTO : NamedEntityDTO
    {
        public UserWithEmailDTO(int id, string name, string email) : base(id, name)
        {
            Email = email;
        }

        public string Email { get; set; }
    }
}