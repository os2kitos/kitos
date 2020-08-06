using System;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainModel.SSO;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.SSO
{
    public interface ISsoUserIdentityRepository
    {
        Maybe<SsoUserIdentity> GetByExternalUuid(Guid externalId);
        Result<SsoUserIdentity, OperationError> AddNew(User user, Guid externalId);
    }
}
