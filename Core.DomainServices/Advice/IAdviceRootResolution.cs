using Core.DomainModel;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Advice
{
    public interface IAdviceRootResolution
    {
        Maybe<IEntityWithAdvices> Resolve(DomainModel.Advice.Advice sourceAdvice);
    }
}
