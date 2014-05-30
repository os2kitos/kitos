namespace Core.DomainModel
{
    public interface IRoleEntity<TRight> : IOptionEntity<TRight>
    {
        bool HasReadAccess { get; set; }
        bool HasWriteAccess { get; set; }
    }
}