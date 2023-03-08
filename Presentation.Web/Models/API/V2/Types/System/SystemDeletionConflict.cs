namespace Presentation.Web.Models.API.V2.Types.System
{
    public enum SystemDeletionConflict
    {
        /// <summary>
        /// The system cannot be deleted because it is still in use in one or more organizations
        /// </summary>
        HasItSystemUsages,
        /// <summary>
        /// The system cannot be deleted because it's set as "parent system" on one or more it-system resources
        /// </summary>
        HasChildSystems,
        /// <summary>
        /// The system cannot be deleted because it's set as "exposing system" for one or more it-interface resources
        /// </summary>
        HasInterfaceExposures
    }
}