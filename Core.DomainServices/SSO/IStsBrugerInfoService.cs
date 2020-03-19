using System;
using Core.DomainModel.Result;

namespace Core.DomainServices.SSO
{
    public interface IStsBrugerInfoService
    {
        Maybe<StsBrugerInfo> GetStsBrugerInfo(Guid uuid);
    }
}