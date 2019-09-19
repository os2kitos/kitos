using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItInterfaceUsageController : BaseApiController
    {
        private readonly IGenericRepository<ItInterfaceUsage> _repository;

        public ItInterfaceUsageController(IGenericRepository<ItInterfaceUsage> repository)
        {
            _repository = repository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceUsageDTO>>))]
        public HttpResponseMessage Get(int usageId, int sysId, int interfaceId)
        {
            try
            {
                var key = ItInterfaceUsage.GetKey(usageId, sysId, interfaceId);
                var item = _repository.GetByKey(key);
                var dto = Map<ItInterfaceUsage, ItInterfaceUsageDTO>(item);
                return Ok(dto);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceUsageDTO>>))]
        public HttpResponseMessage GetByUsage(int usageId)
        {
            try
            {
                var items = _repository.Get(x => x.ItSystemUsageId == usageId);
                var dtos = Map<IEnumerable<ItInterfaceUsage>, IEnumerable<ItInterfaceUsageDTO>>(items);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceUsageDTO>>))]
        public HttpResponseMessage GetByContract(int contractId)
        {
            try
            {
                var items = _repository.Get(x => x.ItContractId == contractId);
                var dtos = Map<IEnumerable<ItInterfaceUsage>, IEnumerable<ItInterfaceUsageDTO>>(items);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Patches IsWishedFor, ItContractId and InfrastructureId only.
        /// If the entry doesn't exist it's created.
        /// </summary>
        /// <param name="usageId"></param>
        /// <param name="sysId"></param>
        /// <param name="organizationId"></param>
        /// <param name="obj"></param>
        /// <param name="interfaceId"></param>
        /// <returns></returns>
        public HttpResponseMessage PatchOrCreate(int usageId, int interfaceId, int sysId, int organizationId, JObject obj)
        {
            try
            {
                var key = ItInterfaceUsage.GetKey(usageId, sysId, interfaceId);
                var item = _repository.GetByKey(key);
                // create if doesn't exists
                if (item == null)
                {
                    item = _repository.Create();
                    item.ItSystemUsageId = usageId;
                    item.ItSystemId = sysId;
                    item.ItInterfaceId = interfaceId;

                    _repository.Insert(item);
                }

                var wishToken = obj.GetValue("isWishedFor");
                if (wishToken != null)
                    item.IsWishedFor = wishToken.Value<bool>();

                var contractToken = obj.GetValue("itContractId");
                if (contractToken != null)
                    item.ItContractId = contractToken.Value<int?>();

                var infraToken = obj.GetValue("infrastructureId");
                if (infraToken != null)
                    item.InfrastructureId = infraToken.Value<int?>();

                _repository.Save();
                var outDto = Map<ItInterfaceUsage, ItInterfaceUsageDTO>(item);
                return Ok(outDto);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
