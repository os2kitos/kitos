namespace Presentation.Web.Models.API.V2.Types.Shared
{
    public enum TrackedEntityTypeChoice
    {
        ItSystem = 0,
        ItSystemUsage = 1,
        ItInterface = 2,
        ItContract = 3,
        DataProcessingRegistration = 4,
        //5 used to be ItProject
        OrganizationUnit = 6,
    }
}