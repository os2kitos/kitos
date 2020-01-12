using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalDataTypesController : LocalOptionBaseController<LocalDataType, DataRow, DataType>
    {
        public LocalDataTypesController(IGenericRepository<LocalDataType> repository, IGenericRepository<DataType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
