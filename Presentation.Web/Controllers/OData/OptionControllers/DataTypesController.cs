using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DataTypesController : BaseOptionController<DataType, DataRow>
    {
        public DataTypesController(IGenericRepository<DataType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}