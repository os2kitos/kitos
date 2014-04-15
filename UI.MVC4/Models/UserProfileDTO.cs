namespace UI.MVC4.Models
{
    public class UserProfileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int? DefaultOrganizationUnitId { get; set; }
    }
}