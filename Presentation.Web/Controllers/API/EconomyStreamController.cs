using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;
using System.Net.Http;
using Core.DomainModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Security;
using System.Web.Http;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Newtonsoft.Json.Linq;
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

        [NonAction]
        public override HttpResponseMessage GetAll(PagingModel<EconomyStream> paging)
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<EconomyStreamDTO>>))]
        public HttpResponseMessage GetExternEconomyStreamForContract(int externPaymentForContractWithId)
        {
            var contract = _contracts
                .AsQueryable()
                .Include(x => x.ExternEconomyStreams)
                .ById(externPaymentForContractWithId);

            if (contract == null)
            {
                return NotFound();
            }

            if (!AllowRead(contract))
            {
                return Forbidden();
            }

            IEnumerable<EconomyStream> result = contract.ExternEconomyStreams;

            var organizationDataReadAccessLevel = GetOrganizationReadAccessLevel(contract.OrganizationId);

            if (organizationDataReadAccessLevel == OrganizationDataReadAccessLevel.Public)
            {
                result = result.Where(x => x.AccessModifier == AccessModifier.Public);
            }

            return Ok(Map(result));
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceDTO>>))]
        public HttpResponseMessage GetInternEconomyStreamForContract(int internPaymentForContractWithId)
        {
            var contract = _contracts
                .AsQueryable()
                .Include(x => x.InternEconomyStreams)
                .ById(internPaymentForContractWithId);

            if (contract == null)
            {
                return NotFound();
            }

            if (!AllowRead(contract))
            {
                return Forbidden();
            }

            IEnumerable<EconomyStream> result = contract.InternEconomyStreams;

            var organizationDataReadAccessLevel = GetOrganizationReadAccessLevel(contract.OrganizationId);

            if (organizationDataReadAccessLevel == OrganizationDataReadAccessLevel.Public)
            {
                result = result.Where(x => x.AccessModifier == AccessModifier.Public);
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

            if (!AllowModify(contract))
            {
                return Forbidden();
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

            var savedItem = PostQuery(stream);

            return Created(Map(savedItem), new Uri(Request.RequestUri + "/" + savedItem.Id));
        }

        protected override EconomyStream PatchQuery(EconomyStream item, JObject obj)
        {
            if (item.InternPaymentFor != null && !AllowModify(item.InternPaymentFor))
                throw new SecurityException();
            if (item.ExternPaymentFor != null && !AllowModify(item.ExternPaymentFor))
                throw new SecurityException();
            return base.PatchQuery(item, obj);
        }
    }
}
