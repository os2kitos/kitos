using Core.DomainModel;

namespace Core.DomainServices.Authorization
{
    public static class GenericQueryCompleteness
    {
        public static bool RequireGenericQueryPostFiltering<T>(CrossOrganizationReadAccess organizationReadAccess)
        {
            if (organizationReadAccess == CrossOrganizationReadAccess.All)
            {
                return false;
            }

            var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(T));
            var hasAccessModifier = typeof(IHasAccessModifier).IsAssignableFrom(typeof(T));
            var contextAware = typeof(IContextAware).IsAssignableFrom(typeof(T));

            //If object is context aware but refinable in the query (has org) then post processing is required on the entity level
            return hasOrg == false && (hasAccessModifier && contextAware);
        }
    }
}
