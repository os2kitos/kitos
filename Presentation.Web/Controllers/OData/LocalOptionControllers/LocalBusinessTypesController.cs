using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalBusinessTypesController : LocalOptionBaseController<LocalBusinessType, ItSystem, BusinessType>
    {
        public LocalBusinessTypesController(IGenericRepository<LocalBusinessType> repository, IAuthenticationService authService, IGenericRepository<BusinessType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
