namespace Core.DomainServices.Model
{
    public interface IReadModelUpdate<in TModel, in TReadModel>
    {
        void Apply(TModel source, TReadModel destination);
    }
}
