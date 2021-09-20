using System;
using Core.Abstractions.Types;


namespace Core.DomainServices.SSO
{
    public interface IStsBrugerInfoService
    {
        Maybe<StsBrugerInfo> GetStsBrugerInfo(Guid uuid, string cvrNumber);
    }
}