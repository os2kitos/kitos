using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class AttachedOptionsController : BaseEntityController<AttachedOption>
    {
        public AttachedOptionsController(
            IGenericRepository<AttachedOption> repository)
               : base(repository)
        {
        }
    }
}