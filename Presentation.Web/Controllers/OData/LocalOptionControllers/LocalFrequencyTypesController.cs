using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalFrequencyTypesController : LocalOptionBaseController<LocalFrequencyType, DataRowUsage, FrequencyType>
    {
        public LocalFrequencyTypesController(IGenericRepository<LocalFrequencyType> repository, IGenericRepository<FrequencyType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
