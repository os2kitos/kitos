namespace Presentation.Web.Models.API.V2.Types.SystemUsage
{
    public enum HostingChoice
    {
        Undecided = 0,
        OnPremise = 1,
        /// <summary>
        /// Cloud etc.
        /// </summary>
        External = 2,
        Hybrid = 3,
    }
}