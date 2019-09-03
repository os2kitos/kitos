namespace Core.DomainServices.Authorization
{
    public enum CrossOrganizationReadAccess
    {
        /// <summary>
        /// No cross-organization read access is allowed
        /// </summary>
        None = 0,
        /// <summary>
        /// Access to all public objects in other organizations
        /// </summary>
        Public = 1,
        /// <summary>
        /// Access to all public and local objects in other organizations
        /// </summary>
        All = 2
    }
}