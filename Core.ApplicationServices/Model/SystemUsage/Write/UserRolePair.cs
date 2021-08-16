using System;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UserRolePair
    {
        public Guid UserUuid { get; set; }
        public Guid RoleUuid { get; set; }
    }
}