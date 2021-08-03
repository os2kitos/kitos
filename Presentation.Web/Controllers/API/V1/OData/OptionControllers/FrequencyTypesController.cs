using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class FrequencyTypesController : BaseOptionController<RelationFrequencyType, SystemRelation>
    {
        public FrequencyTypesController(IGenericRepository<RelationFrequencyType> repository)
            : base(repository)
        {
        }
    }
}