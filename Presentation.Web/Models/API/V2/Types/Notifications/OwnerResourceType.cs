namespace Presentation.Web.Models.API.V2.Types.Notifications
{
    /// <summary>
    /// Type of resource owning the notification, in Domain terms <see cref="Core.DomainModel.Shared.RelatedEntityType"/>
    /// </summary>
    public enum OwnerResourceType
    {
        ItContract = 0,
        ItSystemUsage = 1,
        DataProcessingRegistration = 2
    }
}