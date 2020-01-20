using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class ItContractTemplateTypesController : BaseOptionController<ItContractTemplateType, ItContract>
    {
        public ItContractTemplateTypesController(IGenericRepository<ItContractTemplateType> repository)
            : base(repository)
        {
        }
    }
}