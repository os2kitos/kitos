using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalTsaTypesController : LocalOptionBaseController<LocalTsaType, ItInterface, TsaType>
    {
        public LocalTsaTypesController(IGenericRepository<LocalTsaType> repository, IGenericRepository<TsaType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
