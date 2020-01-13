using Core.DomainModel.ItProject;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalItProjectTypesController : LocalOptionBaseController<LocalItProjectType, ItProject, ItProjectType>
    {
        public LocalItProjectTypesController(IGenericRepository<LocalItProjectType> repository, IGenericRepository<ItProjectType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
