using Core.DomainModel.GDPR;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class DataProcessingRegistrationRolesController : BaseOptionController<DataProcessingRegistrationRole, DataProcessingRegistrationRight>
    {
        public DataProcessingRegistrationRolesController(IGenericRepository<DataProcessingRegistrationRole> repository) : base(repository)
        {

        }
    }
}