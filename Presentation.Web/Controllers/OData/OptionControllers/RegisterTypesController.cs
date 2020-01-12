using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class RegisterTypesController : BaseOptionController<RegisterType, ItSystemUsage>
    {
        public RegisterTypesController(IGenericRepository<RegisterType> repository)
            : base(repository)
        {
        }
    }
}