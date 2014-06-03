using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;
using UI.MVC4.Models.Exceptions;

namespace UI.MVC4.Controllers.API
{
    public class CustomAgreementElementController : GenericApiController<CustomAgreementElement, CustomAgreementElementDTO>
    {
        public CustomAgreementElementController(IGenericRepository<CustomAgreementElement> repository) 
            : base(repository)
        {
        }

        protected override CustomAgreementElement PostQuery(CustomAgreementElement item)
        {
            if (Repository.Get(x => x.Name == item.Name).Any())
                throw new ConflictException("Element with that name already exist!");
            
            return base.PostQuery(item);
        }

        public HttpResponseMessage GetByContractId(int id, [FromUri] bool? contractId)
        {
            var items = Repository.Get(x => x.ItContractId == id);

            return Ok(Map(items));
        }
    }
}