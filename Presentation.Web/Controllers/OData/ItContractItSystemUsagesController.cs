using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    /// <summary>
    /// Gives access to relations between ItContract and ItSystemUsage
    /// Primarily used for reporting
    /// </summary>
    public class ItContractItSystemUsagesController : BaseController<ItContractItSystemUsage>
    {
        public ItContractItSystemUsagesController(IGenericRepository<ItContractItSystemUsage> repository)
            : base(repository)
        {
        }
    }
}