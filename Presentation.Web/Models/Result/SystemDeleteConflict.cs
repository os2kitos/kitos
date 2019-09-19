namespace Presentation.Web.Models.Result
{
    public enum SystemDeleteConflict
    {
        InUse,
        HasChildren,
        HasInterfaceExhibits
    }
}