namespace Core.DomainModel
{
    public interface IEntity<T>
    {
        T Id { get; set; }
    }
}