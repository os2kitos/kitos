using System;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.SSO;


namespace Core.DomainServices.Repositories.SSO
{
    public interface ISsoUserIdentityRepository
    {
        Maybe<SsoUserIdentity> GetByExternalUuid(Guid externalId);
        Result<SsoUserIdentity, OperationError> AddNew(User user, Guid externalId);
        void DeleteIdentitiesForUser(User user);
    }
}
