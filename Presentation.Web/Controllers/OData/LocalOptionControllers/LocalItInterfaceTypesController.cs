using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalItInterfaceTypesController : LocalOptionBaseController<LocalItInterfaceType, ItInterface, ItInterfaceType>
    {
        public LocalItInterfaceTypesController(IGenericRepository<LocalItInterfaceType> repository, IGenericRepository<ItInterfaceType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
