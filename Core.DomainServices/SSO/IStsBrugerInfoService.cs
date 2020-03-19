using System;

namespace Core.DomainServices.SSO
{
    public interface IStsBrugerInfoService
    {
        StsBrugerInfo GetStsBrugerInfo(Guid uuid);
    }
}