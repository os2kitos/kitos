using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    //TODO: Remove this class - no benefit
    [MigratedToNewAuthorizationContext]
    public class GenericContextAwareApiController<TModel, TDto> : GenericApiController<TModel, TDto>
        where TModel : Entity, IContextAware
    {
        public GenericContextAwareApiController(IGenericRepository<TModel> repository, IAuthorizationContext authorizationContext = null)
            : base(repository, authorizationContext)
        {
        }
    }
}
