using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.Contract;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class HandoverTrialController : GenericApiController<HandoverTrial, HandoverTrialDTO>
    {
        private readonly IItContractRepository _contractRepository;

        public HandoverTrialController(
            IGenericRepository<HandoverTrial> repository,
            IItContractRepository contractRepository)
            : base(repository)
        {
            _contractRepository = contractRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<HandoverTrialDTO>>))]
        public HttpResponseMessage GetByContractid(int id, bool? byContract)
        {
            var itContract = _contractRepository.GetById(id);
            
            if (itContract == null)
                return NotFound();
            
            if (!AllowRead(itContract))
                return Forbidden();

            var query = Repository.Get(x => x.ItContractId == id);

            var dtos = Map(query);
            return Ok(dtos);
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<HandoverTrial, ItContract>(x => _contractRepository.GetById(x.ItContractId), base.GetCrudAuthorization());
        }

        protected override IQueryable<HandoverTrial> GetAllQuery()
        {
            var query = Repository.AsQueryable();
            if (AuthorizationContext.GetCrossOrganizationReadAccess() == CrossOrganizationDataReadAccessLevel.All)
            {
                return query;
            }

            var organizationIds = UserContext.OrganizationIds.ToList();

            return query.Where(x => organizationIds.Contains(x.ItContract.OrganizationId));
        }
    }
}
