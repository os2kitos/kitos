using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class TsaTypesController : BaseOptionController<TsaType, ItInterface>
    {
        public TsaTypesController(IGenericRepository<TsaType> repository)
            : base(repository)
        {
        }
    }
}