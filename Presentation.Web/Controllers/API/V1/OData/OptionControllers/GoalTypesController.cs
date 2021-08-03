using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class GoalTypesController : BaseOptionController<GoalType, Goal>
    {
        public GoalTypesController(IGenericRepository<GoalType> repository)
            : base(repository)
        {
        }
    }
}