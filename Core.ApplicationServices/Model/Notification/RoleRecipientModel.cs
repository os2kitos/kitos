namespace Core.ApplicationServices.Model.Notification
{
    public class RoleRecipientModel
    {
        public RoleRecipientModel(int roleId)
        {
            RoleId = roleId;
        }

        public int RoleId { get; }
    }
}
