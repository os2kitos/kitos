namespace Core.DomainModel
{
    public interface IOwnedByOrganization
    {
        /// <summary>
        ///     Gets or sets the organization identifier in which this project was created.
        /// </summary>
        /// <value>
        ///     The organization identifier.
        /// </value>
        int OrganizationId { get; set; }

        /// <summary>
        ///     Gets or sets the organization in which this project was created.
        /// </summary>
        /// <value>
        ///     The organization.
        /// </value>
        Organization.Organization Organization { get; set; }
    }
}
