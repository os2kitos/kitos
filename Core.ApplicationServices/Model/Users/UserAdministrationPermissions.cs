namespace Core.ApplicationServices.Model.Users
{
    public class UserAdministrationPermissions
    {
        public bool AllowDelete { get; }

        public UserAdministrationPermissions(bool allowDelete)
        {
            AllowDelete = allowDelete;
        }
    }
}
