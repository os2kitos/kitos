using Core.Abstractions.Types;
using Core.DomainModel;


namespace Core.DomainServices.Advice
{
    public interface IAdviceRootResolution
    {
        Maybe<IEntityWithAdvices> Resolve(DomainModel.Advice.Advice sourceAdvice);
    }
}
