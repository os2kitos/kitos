using System;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class RoleRecipientModificationParameters
    {
        public RoleRecipientModificationParameters(Guid roleUuid)
        {
            RoleUuid = roleUuid;
        }

        public Guid RoleUuid { get; }
    }
}
