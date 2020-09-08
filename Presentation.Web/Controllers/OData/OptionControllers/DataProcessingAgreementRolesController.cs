using Core.DomainModel.GDPR;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class DataProcessingAgreementRolesController : BaseOptionController<DataProcessingAgreementRole, DataProcessingAgreementRight>
    {
        public DataProcessingAgreementRolesController(IGenericRepository<DataProcessingAgreementRole> repository) : base(repository)
        {

        }
    }
}