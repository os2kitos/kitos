﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
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
            var items = Repository.Get(x => x.ItContractId == id);

            return Ok(Map(items));
        }

        protected override bool AllowCreate<T>(IEntity entity)
        {
            if (entity is PaymentMilestone relation)
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
            if (entity is PaymentMilestone relation)
            {
                return condition.Invoke(relation.ItContract);
            }

            return false;
        }
    }
}
