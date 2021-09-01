using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class BusinessTypesController : BaseOptionController<BusinessType, ItSystem>
    {
        public BusinessTypesController(IGenericRepository<BusinessType> repository)
            : base(repository)
        {
        }
    }
}