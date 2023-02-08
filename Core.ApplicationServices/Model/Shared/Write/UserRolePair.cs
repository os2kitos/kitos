using System;

namespace Core.ApplicationServices.Model.Shared.Write
{
    public class UserRolePair
    {
        public Guid UserUuid { get; }
        public Guid RoleUuid { get; }

        public UserRolePair(Guid userUuid, Guid roleUuid)
        {
            UserUuid = userUuid;
            RoleUuid = roleUuid;
        }

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