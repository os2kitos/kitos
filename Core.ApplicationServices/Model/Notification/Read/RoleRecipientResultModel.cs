using Core.DomainModel;

namespace Core.ApplicationServices.Model.Notification.Read
{
    public class RoleRecipientResultModel
    {
        public RoleRecipientResultModel(IRoleEntity role)
        {
            Role = role;
        }

        public IRoleEntity Role{ get; }
    }
}
