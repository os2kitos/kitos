using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
    public class HandoverTrialController : GenericContextAwareApiController<HandoverTrial, HandoverTrialDTO>
    {
        private readonly IItContractRepository _contractRepository;

        public HandoverTrialController(
            IGenericRepository<HandoverTrial> repository,
            IItContractRepository contractRepository,
            IAuthorizationContext authorization)
            : base(repository, authorization)
        {
            _contractRepository = contractRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<HandoverTrialDTO>>))]
        public HttpResponseMessage GetByContractid(int id, bool? byContract)
        {
            var query = Repository.Get(x => x.ItContractId == id);
            var dtos = Map(query);
            return Ok(dtos);
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<HandoverTrial>(x => _contractRepository.GetById(x.ItContractId), base.GetCrudAuthorization());
        }
    }
}
