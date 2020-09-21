using Core.DomainModel.GDPR;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class DataProcessingTransferReasonOptionsController : BaseOptionController<DataProcessingTransferReasonOption, DataProcessingAgreement>
    {
        public DataProcessingTransferReasonOptionsController(IGenericRepository<DataProcessingTransferReasonOption> repository)
            : base(repository)
        {
        }
    }
}
