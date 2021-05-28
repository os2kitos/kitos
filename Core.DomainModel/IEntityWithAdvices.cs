namespace Core.DomainModel
{
    /// <summary>
    /// Simple marker interface for target if <see cref="Advice"/> which has no hard coupling to the target reference
    /// </summary>
    public interface IEntityWithAdvices: IEntity
    {
    }
}
