using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Infrastructure.Services.DomainEvents;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItSystemUsageRightsController : GenericRightsController<ItSystemUsage, ItSystemRight, ItSystemRole>
    {
        public ItSystemUsageRightsController(
            IGenericRepository<ItSystemRight> rightRepository, 
            IGenericRepository<ItSystemUsage> objectRepository,
            IDomainEvents domainEvents)
            : base(rightRepository, objectRepository, domainEvents)
        { }
    }
}
