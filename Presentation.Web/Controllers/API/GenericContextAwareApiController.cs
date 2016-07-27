using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
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
            // local admin have write access if the obj is in context
            if (obj.IsInContext(organizationId) &&
                user.OrganizationRights.Any(x => x.OrganizationId == organizationId && x.Role == OrganizationRole.LocalAdmin))
                return true;

            return base.HasWriteAccess(obj, user, organizationId);
        }
    }
}
