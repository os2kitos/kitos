namespace Core.DomainModel
{
    public class OrganizationRight : IRight<OrganizationUnit, OrganizationRole>
    {
        public int User_Id { get; set; }
        public int Role_Id { get; set; }
        public int Object_Id { get; set; }
        public User User { get; set; }
        public OrganizationRole Role { get; set; }
        public OrganizationUnit Object { get; set; }
    }
}