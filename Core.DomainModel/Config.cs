namespace Core.DomainModel
{
    /// <summary>
    /// Configuration of KITOS for an organization
    /// </summary>
    public class Config : Entity, IContextAware
    {
        /* SHOW/HIDE MODULES */
        public bool ShowItProjectModule { get; set; }
        public bool ShowItSystemModule { get; set; }
        public bool ShowItContractModule { get; set; }

        /* SHOW/HIDE 'IT' PREFIX */
        public bool ShowItProjectPrefix { get; set; }
        public bool ShowItSystemPrefix { get; set; }
        public bool ShowItContractPrefix { get; set; }

        /* IT SUPPORT */
        public int ItSupportModuleNameId { get; set; }
        public string ItSupportGuide { get; set; }
        public bool ShowTabOverview { get; set; }
        public bool ShowColumnTechnology { get; set; }
        public bool ShowColumnUsage { get; set; }

        public virtual Organization.Organization Organization { get; set; }

        public static Config Default(User objectOwner)
        {
            return new Config
            {
                    ShowItContractModule = true,
                    ShowItProjectModule = true,
                    ShowItSystemModule = true,
                    ShowItContractPrefix = true,
                    ShowItProjectPrefix = true,
                    ShowItSystemPrefix = true,
                    ShowColumnTechnology = true,
                    ShowColumnUsage = true,
                    ShowTabOverview = true,
                    ObjectOwner = objectOwner,
                    LastChangedByUser = objectOwner
                };
        }

        /// <summary>
        /// Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///   <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        public bool IsInContext(int organizationId)
        {
            // this is a 1:0-1 relation to Organization so this.Id actually is the OrganizationId
            return Id == organizationId;
        }
    }
}
