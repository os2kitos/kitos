using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class ItContractTypesController : BaseOptionController<ItContractType, ItContract>
    {
        public ItContractTypesController(IGenericRepository<ItContractType> repository)
            : base(repository)
        {
        }
    }
}