namespace Core.ApplicationServices.Model.System
{
    public enum DeleteResult
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
