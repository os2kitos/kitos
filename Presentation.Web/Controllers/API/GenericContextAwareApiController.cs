using Core.DomainModel;
using Core.DomainServices;

namespace Presentation.Web.Controllers.API
{
    public class GenericContextAwareApiController<TModel, TDto> : GenericApiController<TModel, TDto>
        where TModel : Entity, IContextAware
    {
        public GenericContextAwareApiController(IGenericRepository<TModel> repository)
            : base(repository)
        {
        }

        protected override bool HasWriteAccess(TModel obj, User user, int organizationId)
        {
            return base.HasWriteAccess(obj, user, organizationId);
        }
    }
}
