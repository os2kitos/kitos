using System;
using Core.DomainModel;

namespace Core.DomainServices.Context
{
    public class ActiveUserContext
    {
        public int ActiveOrganizationId { get; }
        public User UserEntity { get; }

        public ActiveUserContext(int activeOrganizationId, User userEntity)
        {
            ActiveOrganizationId = activeOrganizationId;
            UserEntity = userEntity ?? throw new ArgumentNullException(nameof(userEntity));
        }
    }
}
