using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalBusinessTypesController : LocalOptionBaseController<LocalBusinessType, ItSystem, BusinessType>
    {
        public LocalBusinessTypesController(IGenericRepository<LocalBusinessType> repository, IGenericRepository<BusinessType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
