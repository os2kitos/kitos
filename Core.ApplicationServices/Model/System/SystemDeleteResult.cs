namespace Core.ApplicationServices.Model.System
{
    public enum SystemDeleteResult
    {
        Ok,
        Forbidden,
        InUse,
        HasChildren,
        HasInterfaceExhibits,
        NotFound,
        UnknownError
    }
}
