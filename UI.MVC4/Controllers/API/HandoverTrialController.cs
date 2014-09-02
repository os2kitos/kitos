using System.Net.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class HandoverTrialController : GenericApiController<HandoverTrial, HandoverTrialDTO>
    {
        public HandoverTrialController(IGenericRepository<HandoverTrial> repository) 
            : base(repository)
        {
        }

        public HttpResponseMessage GetByContractid(int id, bool? byContract)
        {
            var query = Repository.Get(x => x.ItContractId == id);
            var dtos = Map(query);
            return Ok(dtos);
        }
    }
}