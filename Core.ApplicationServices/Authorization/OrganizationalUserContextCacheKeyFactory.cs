namespace Core.ApplicationServices.Authorization
{
    public static class OrganizationalUserContextCacheKeyFactory
    {
        public static string Create(int userId)
        {
            return $"{nameof(IOrganizationalUserContext)}_{userId}";
        }
    }
}
