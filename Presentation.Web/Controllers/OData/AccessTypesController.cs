using Core.ApplicationServices;
using Core.DomainServices;
using Core.DomainModel.ItSystem;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class AccessTypesController : BaseEntityController<AccessType>
    {

        public AccessTypesController(IGenericRepository<AccessType> repository, IAuthenticationService authService)
            : base(repository)
        {
        }
    }
}