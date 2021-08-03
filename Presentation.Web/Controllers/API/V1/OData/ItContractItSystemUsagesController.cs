using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    /// <summary>
    /// Gives access to relations between ItContract and ItSystemUsage
    /// Primarily used for reporting
    /// </summary>
    [PublicApi]
    public class ItContractItSystemUsagesController : BaseController<ItContractItSystemUsage>
    {
        public ItContractItSystemUsagesController(IGenericRepository<ItContractItSystemUsage> repository)
            : base(repository)
        {
        }
    }
}