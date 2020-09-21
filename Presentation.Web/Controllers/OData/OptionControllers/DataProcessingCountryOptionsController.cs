using Core.DomainModel.GDPR;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class DataProcessingCountryOptionsController : BaseOptionController<DataProcessingCountryOption, DataProcessingAgreement>
    {
        public DataProcessingCountryOptionsController(IGenericRepository<DataProcessingCountryOption> repository)
            : base(repository)
        {
        }
    }
}
