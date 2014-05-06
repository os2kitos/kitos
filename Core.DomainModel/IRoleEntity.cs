namespace Core.DomainModel
{
    public interface IRoleEntity
    {
        bool HasReadAccess { get; set; }
        bool HasWriteAccess { get; set; }
    }
}