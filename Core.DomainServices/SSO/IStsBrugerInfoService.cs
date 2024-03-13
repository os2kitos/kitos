using System;
using Core.Abstractions.Types;


namespace Core.DomainServices.SSO
{
    public interface IStsBrugerInfoService
    {
        Result<StsBrugerInfo, string> GetStsBrugerInfo(Guid uuid, string cvrNumber);
    }
}