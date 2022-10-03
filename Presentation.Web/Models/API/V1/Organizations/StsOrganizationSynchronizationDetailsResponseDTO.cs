namespace Presentation.Web.Models.API.V1.Organizations
{
    public class StsOrganizationSynchronizationDetailsResponseDTO
    {
        public bool Connected { get; set; }
        public int? SynchronizationDepth { get; set; }
        public bool CanCreateConnection { get; set; }
        public bool CanUpdateConnection { get; set; }
        public bool CanDeleteConnection { get; set; }
    }
}