namespace Core.DomainModel
{
    public interface IRoleEntity<T> : IOptionEntity<T>
    {
        bool HasReadAccess { get; set; }
        bool HasWriteAccess { get; set; }
    }
}