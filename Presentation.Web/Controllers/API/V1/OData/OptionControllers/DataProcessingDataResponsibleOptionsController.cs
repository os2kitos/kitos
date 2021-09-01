using Core.DomainModel.GDPR;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class DataProcessingDataResponsibleOptionsController : BaseOptionController<DataProcessingDataResponsibleOption, DataProcessingRegistration>
    {
        public DataProcessingDataResponsibleOptionsController(IGenericRepository<DataProcessingDataResponsibleOption> repository)
            : base(repository)
        {
        }
    }
}
