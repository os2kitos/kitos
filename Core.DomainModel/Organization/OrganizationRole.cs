namespace Core.DomainModel.Organization
{
    public enum OrganizationRole
    {
        /// <summary>
        /// Has read access to everything within the organization,
        /// but not write access
        /// </summary>
        User,
        /// <summary>
        /// Has write access to everything within the organization
        /// </summary>
        LocalAdmin,
        /// <summary>
        /// Has write access to everything within the organization module
        /// </summary>
        OrganizationModuleAdmin,
        /// <summary>
        /// Has write access to everything within the project module
        /// </summary>
        ProjectModuleAdmin,
        /// <summary>
        /// Has write access to everything within the system module
        /// </summary>
        SystemModuleAdmin,
        /// <summary>
        /// Has write access to everything within the contract module
        /// </summary>
        ContractModuleAdmin,
        /// <summary>
        /// Has write access to organization reports.
        /// </summary>
        ReportModuleAdmin,
        /// <summary>
        /// Has only read access.
        /// </summary>
        ReadOnly,
        /// <summary>
        /// God mode
        /// </summary>
        GlobalAdmin

    }
}
