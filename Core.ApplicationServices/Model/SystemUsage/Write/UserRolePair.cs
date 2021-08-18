using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UserRolePair
    {
        public Guid UserUuid { get; set; }
        public Guid RoleUuid { get; set; }

        protected bool Equals(UserRolePair other)
        {
            return UserUuid.Equals(other.UserUuid) && RoleUuid.Equals(other.RoleUuid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserRolePair)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (UserUuid.GetHashCode() * 397) ^ RoleUuid.GetHashCode();
            }
        }

    }
}