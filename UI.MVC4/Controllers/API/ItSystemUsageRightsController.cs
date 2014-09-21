using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemUsageRightsController : GenericRightsController<ItSystemUsage, ItSystemRight, ItSystemRole>
    {
        public ItSystemUsageRightsController(IGenericRepository<ItSystemRight> rightRepository, IGenericRepository<ItSystemUsage> objectRepository) : base(rightRepository, objectRepository)
        {
        }
    }
}
