using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class ItSystemUsageRightsController : GenericRightsController<ItSystemUsage, ItSystemRight, ItSystemRole>
    {
        public ItSystemUsageRightsController(
            IGenericRepository<ItSystemRight> rightRepository, 
            IGenericRepository<ItSystemUsage> objectRepository,
            IAuthorizationContext authorizationContext) 
            : base(rightRepository, objectRepository, authorizationContext)
        { }
    }
}
