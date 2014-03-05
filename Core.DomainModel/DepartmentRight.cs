namespace Core.DomainModel
{
    public class DepartmentRight : IRight<Department, DepartmentRole>
    {
        public int User_Id { get; set; }
        public int Role_Id { get; set; }
        public int Object_Id { get; set; }
        public User User { get; set; }
        public DepartmentRole Role { get; set; }
        public Department Object { get; set; }
    }
}