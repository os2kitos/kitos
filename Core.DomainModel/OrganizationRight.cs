namespace Core.DomainModel
{
    public class OrganizationRight : IRight<OrganizationUnit, OrganizationRole>
    {
        public int User_Id { get; set; }
        public int Role_Id { get; set; }
        public int Object_Id { get; set; }
        public virtual User User { get; set; }
        public virtual OrganizationRole Role { get; set; }
        public virtual OrganizationUnit Object { get; set; }
    }
}