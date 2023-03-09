namespace Core.ApplicationServices.Model.System
{
    public enum SystemDeletionConflict
    {
        InUse,
        HasChildren,
        HasInterfaceExhibits,
    }
}
