using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
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
    [MigratedToNewAuthorizationContext]
    public class PaymentMilestoneController : GenericContextAwareApiController<PaymentMilestone, PaymentMilestoneDTO>
    {
        private readonly IItContractRepository _contractRepository;

        public PaymentMilestoneController(
            IGenericRepository<PaymentMilestone> repository,
            IItContractRepository contractRepository,
            IAuthorizationContext authorization)
            : base(repository, authorization)
        {
            _contractRepository = contractRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<PaymentMilestoneDTO>>))]
        public HttpResponseMessage GetByContractId(int id, [FromUri] bool? contract)
        {
            var items =
                Repository.Get(x => x.ItContractId == id)
                    .Where(AllowRead);

            return Ok(Map(items));
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<PaymentMilestone>(x => _contractRepository.GetById(x.ItContractId), base.GetCrudAuthorization());
        }
    }
}
