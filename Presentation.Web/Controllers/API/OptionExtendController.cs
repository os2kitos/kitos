using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class OptionExtendController : GenericOptionApiController<OptionExtendType, ItContract, OptionDTO>
    {
        public OptionExtendController(IGenericRepository<OptionExtendType> repository)
            : base(repository)
        {
        }
    }
}
