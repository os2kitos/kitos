using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class BusinessTypesController : BaseOptionController<BusinessType, ItSystem>
    {
        public BusinessTypesController(IGenericRepository<BusinessType> repository)
            : base(repository)
        {
        }
    }
}