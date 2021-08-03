namespace Presentation.Web.Models.API.V1.ItSystem
{
    public enum SystemDeleteConflict
    {
        InUse,
        HasChildren,
        HasInterfaceExhibits
    }
}