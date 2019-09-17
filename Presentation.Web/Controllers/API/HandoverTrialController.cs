using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Core.DomainServices.Repositories.Contract;
using Presentation.Web.Infrastructure.Attributes;
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

        protected override bool AllowCreate<T>(IEntity entity)
        {
            if (entity is HandoverTrial relation)
            {
                var contract = _contractRepository.GetById(relation.ItContractId);
                return contract != null && base.AllowModify(contract);
            }
            return false;
        }

        protected override bool AllowModify(IEntity entity)
        {
            return GeAuthorizationFromRoot(entity, base.AllowModify);
        }

        protected override bool AllowDelete(IEntity entity)
        {
            //Check if modification, not deletion, of parent usage (the root aggregate) is allowed 
            return GeAuthorizationFromRoot(entity, base.AllowModify);
        }

        protected override bool AllowRead(IEntity entity)
        {
            return GeAuthorizationFromRoot(entity, base.AllowRead);
        }

        private static bool GeAuthorizationFromRoot(IEntity entity, Predicate<ItContract> condition)
        {
            if (entity is HandoverTrial relation)
            {
                return condition.Invoke(relation.ItContract);
            }

            return false;
        }
    }
}
