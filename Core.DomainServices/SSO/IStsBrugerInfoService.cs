namespace Core.DomainServices.SSO
{
    public interface IStsBrugerInfoService
    {
        StsBrugerInfo GetStsBrugerInfo(string uuid);
    }
}