using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class AgreementElementController : GenericOptionApiController<AgreementElementType, ItContract, OptionDTO>
    {
        /// <summary>
        /// Inherits from base controller, provides Agreement elements(aftaleelementer) for it contracts
        /// </summary>
        /// <param name="repository"></param>
        public AgreementElementController(IGenericRepository<AgreementElementType> repository)
            : base(repository)
        {
        }
    }
}
