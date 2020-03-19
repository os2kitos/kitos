using System;
using Core.DomainModel.Result;
using Core.DomainModel.SSO;

namespace Core.DomainServices.Repositories.SSO
{
    public interface ISsoUserIdentityRepository
    {
        Maybe<SsoUserIdentity> GetByExternalUuid(Guid externalId);
    }
}
