using Core.ApplicationServices;
using Core.DomainModel.AdviceSent;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class AdviceSentController : BaseEntityController<AdviceSent>
    {
        public AdviceSentController(IGenericRepository<AdviceSent> repository, IAuthenticationService authService): 
        base(repository,authService){ }
    }
}