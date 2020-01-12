using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class TerminationDeadlineTypesController : BaseOptionController<TerminationDeadlineType, ItContract>
    {
        public TerminationDeadlineTypesController(IGenericRepository<TerminationDeadlineType> repository)
            : base(repository)
        {
        }
    }
}