using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Core.DomainServices.Repositories.Contract;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class PaymentMilestoneController : GenericApiController<PaymentMilestone, PaymentMilestoneDTO>
    {
        private readonly IItContractRepository _contractRepository;

        public PaymentMilestoneController(IGenericRepository<PaymentMilestone> repository, IItContractRepository contractRepository)
            : base(repository)
        {
            _contractRepository = contractRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<PaymentMilestoneDTO>>))]
        public HttpResponseMessage GetByContractId(int id, [FromUri] bool? contract)
        {
            var itContract = _contractRepository.GetById(id);
            if (contract == null)
                return NotFound();
            
            if (!AllowRead(itContract))
                return Forbidden();

            var items = Repository.Get(x => x.ItContractId == id);

            return Ok(Map(items));
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<PaymentMilestone, ItContract>(x => _contractRepository.GetById(x.ItContractId), base.GetCrudAuthorization());
        }
    }
}
