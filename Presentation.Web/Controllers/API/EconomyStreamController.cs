using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;
using System.Net.Http;
using Core.DomainModel;
using System;
using System.Collections.Generic;
using System.Net;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class EconomyStreamController : GenericApiController<EconomyStream, EconomyStreamDTO>
    {
        private readonly IGenericRepository<ItContract> _contracts;

        public EconomyStreamController(
            IGenericRepository<EconomyStream> repository,
            IGenericRepository<ItContract> contracts)
            : base(repository)
        {
            _contracts = contracts;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<EconomyStreamDTO>>))]
        public HttpResponseMessage GetExternEconomyStreamForContract(int externPaymentForContractWithId)
        {
            var result = Repository.AsQueryable().Where(e => e.ExternPaymentForId == externPaymentForContractWithId);
            var currentOrgId = ActiveOrganizationId;

            var crossOrganizationReadAccessLevel = GetCrossOrganizationReadAccessLevel();

            if (crossOrganizationReadAccessLevel >= CrossOrganizationDataReadAccessLevel.Public)
            {
                if (crossOrganizationReadAccessLevel < CrossOrganizationDataReadAccessLevel.All && result.Any())
                {
                    // all users may view economy streams marked Public or if they are part of the organization
                    result = result.Where(x => x.AccessModifier == AccessModifier.Public || x.ExternPaymentFor.OrganizationId == currentOrgId);
                }
            }
            else
            {
                result = result.Where(x => x.OrganizationUnit.OrganizationId == currentOrgId);
            }

            return Ok(Map(result));
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceDTO>>))]
        public HttpResponseMessage GetInternEconomyStreamForContract(int internPaymentForContractWithId)
        {
            var result = Repository.AsQueryable().Where(e => e.InternPaymentForId == internPaymentForContractWithId);
            var currentOrgId = ActiveOrganizationId;

            var crossOrganizationReadAccessLevel = GetCrossOrganizationReadAccessLevel();

            if (crossOrganizationReadAccessLevel >= CrossOrganizationDataReadAccessLevel.Public)
            {
                if (crossOrganizationReadAccessLevel < CrossOrganizationDataReadAccessLevel.All && result.Any())
                {
                    // all users may view economy streams marked Public or if they are part of the organization
                    result = result.Where(x => x.AccessModifier == AccessModifier.Public || x.InternPaymentFor.OrganizationId == currentOrgId);
                }
            }
            else
            {
                result = result.Where(x => x.OrganizationUnit.OrganizationId == currentOrgId);
            }

            return Ok(Map(result));
        }

        public HttpResponseMessage Post(int contractId, EconomyStreamDTO streamDTO)
        {
            var contract = _contracts.GetByKey(contractId);

            if (contract == null)
            {
                return NotFound();
            }

            var stream = Map<EconomyStreamDTO, EconomyStream>(streamDTO);

            if (streamDTO.ExternPaymentForId != null)
            {
                stream.ExternPaymentFor = contract;
            }
            else
            {
                stream.InternPaymentFor = contract;
            }

            if (!AllowCreate<EconomyStream>(stream))
            {
                return Forbidden();
            }

            var savedItem = PostQuery(stream);

            return Created(Map(savedItem), new Uri(Request.RequestUri + "/" + savedItem.Id));
        }
    }
}
