namespace Core.DomainModel.Organization
{

    /// <summary>
    /// Determines the properties of the organization's connection to STS Organisation
    /// </summary>
    public class StsOrganizationConnection : Entity, IOwnedByOrganization
    {
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
        public bool Connected { get; set; }
        /// <summary>
        /// Determines the optional synchronization depth used during synchronization from STS Organisation
        /// </summary>
        public int? SynchronizationDepth { get; set; }
        //TODO https://os2web.atlassian.net/browse/KITOSUDV-3317 adds the change logs here
        //TODO: https://os2web.atlassian.net/browse/KITOSUDV-3312 adds automatic subscription here
    }
}
