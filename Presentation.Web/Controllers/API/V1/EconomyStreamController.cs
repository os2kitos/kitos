using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Security;
using System.Web.Http;
using Core.DomainModel.Events;
using Core.DomainServices.Extensions;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
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
        public override HttpResponseMessage GetAll(PagingModel<EconomyStream> paging) => throw new NotSupportedException();

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

            IEnumerable<EconomyStream> result = contract.ExternEconomyStreams.ToList();

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

            IEnumerable<EconomyStream> result = contract.InternEconomyStreams.ToList();

            return Ok(Map(result));
        }

        public override HttpResponseMessage Post(int contractId, EconomyStreamDTO streamDTO)
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

            DomainEvents.Raise(new EntityUpdatedEvent<ItContract>(contract));

            return Created(Map(savedItem), new Uri(Request.RequestUri + "/" + savedItem.Id));
        }

        protected override void RaiseUpdated(EconomyStream item)
        {
            RaiseContractUpdated(item);
        }

        protected override void RaiseDeleted(EconomyStream entity)
        {
            RaiseContractUpdated(entity);
        }

        private void RaiseContractUpdated(EconomyStream item)
        {
            var contract = item.InternPaymentFor ?? item.ExternPaymentFor;
            if (contract != null)
                DomainEvents.Raise(new EntityUpdatedEvent<ItContract>(contract));
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
