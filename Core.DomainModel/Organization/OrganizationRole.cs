namespace Core.DomainModel.Organization
{
    public enum OrganizationRole
    {
        /// <summary>
        /// Has read access to everything within the organization,
        /// but not write access
        /// </summary>
        User = 0,
        /// <summary>
        /// Has write access to everything within the organization
        /// </summary>
        LocalAdmin = 1,
        /// <summary>
        /// Has write access to everything within the organization module
        /// </summary>
        OrganizationModuleAdmin = 2,
        /// <summary>
        /// Has write access to everything within the project module
        /// </summary>
        ProjectModuleAdmin = 3,
        /// <summary>
        /// Has write access to everything within the system module
        /// </summary>
        SystemModuleAdmin = 4,
        /// <summary>
        /// Has write access to everything within the contract module
        /// </summary>
        ContractModuleAdmin = 5,
        //NOTE: a jump from 5-7 due to the old report admin
        /// <summary>
        /// God mode
        /// </summary>
        GlobalAdmin = 7,
        /// <summary>
        /// Access based on the rightsholder of the entity.
        /// </summary>
        RightsHolderAccess = 8

    }
}
