using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class DataTypesController : BaseOptionController<DataType, DataRow>
    {
        public DataTypesController(IGenericRepository<DataType> repository)
            : base(repository)
        {
        }
    }
}