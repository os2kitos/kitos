namespace Core.ApplicationServices.Authorization
{
    public interface IAuthorizationPolicy<in T>
    {
        bool Allow(T target);
    }
}
