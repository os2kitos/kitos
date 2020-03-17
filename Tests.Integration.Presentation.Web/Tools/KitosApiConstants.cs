namespace Tests.Integration.Presentation.Web.Tools
{
    public static class KitosApiConstants
    {
        /// <summary>
        /// In some API endpoints, the organization ID is required but not used e.g. (GET api/system/{id} and DELETE ... and so on) in those cases, use this constant to clearly marked that issue.
        /// </summary>
        public const string UnusedOrganizationIdParameter = "organizationId=-1";
    }
}
