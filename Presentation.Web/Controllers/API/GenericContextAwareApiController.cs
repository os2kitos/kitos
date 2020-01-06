using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    //TODO: Kill this one - it does not provide any meaningful abstraction
    [ControllerEvaluationCompleted]
    public class GenericContextAwareApiController<TModel, TDto> : GenericApiController<TModel, TDto>
        where TModel : Entity, IContextAware
    {
        public GenericContextAwareApiController(IGenericRepository<TModel> repository, IAuthorizationContext authorizationContext = null)
            : base(repository, authorizationContext)
        {
        }
    }
}
