using Core.DomainModel.GDPR;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class DataProcessingCountryOptionsController : BaseOptionController<DataProcessingCountryOption, DataProcessingRegistration>
    {
        public DataProcessingCountryOptionsController(IGenericRepository<DataProcessingCountryOption> repository)
            : base(repository)
        {
        }
    }
}
