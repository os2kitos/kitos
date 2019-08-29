using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Authorization.Context;

namespace Presentation.Web.Controllers.API
{
    public class GenericContextAwareApiController<TModel, TDto> : GenericApiController<TModel, TDto>
        where TModel : Entity, IContextAware
    {
        public GenericContextAwareApiController(IGenericRepository<TModel> repository, IAuthorizationContext authorizationContext = null)
            : base(repository, authorizationContext)
        {
        }
    }
}
