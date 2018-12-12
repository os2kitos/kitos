using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class AgreementElementController : GenericOptionApiController<AgreementElementType, ItContract, OptionDTO>
    {
        /// <summary>
        /// Nedarver fra base controlleren, udstiller Aftaleelementer of it kontrakter
        /// </summary>
        /// <param name="repository"></param>
        public AgreementElementController(IGenericRepository<AgreementElementType> repository)
            : base(repository)
        {
        }
    }
}
