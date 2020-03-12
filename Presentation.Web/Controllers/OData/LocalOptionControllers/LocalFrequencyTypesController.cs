using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalFrequencyTypesController : LocalOptionBaseController<LocalRelationFrequencyType, SystemRelation, RelationFrequencyType>
    {
        public LocalFrequencyTypesController(IGenericRepository<LocalRelationFrequencyType> repository, IGenericRepository<RelationFrequencyType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
