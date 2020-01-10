using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Repositories.SystemUsage;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class ItInterfaceUsageController : BaseApiController
    {
        private readonly IGenericRepository<ItInterfaceUsage> _repository;
        private readonly IItSystemUsageRepository _itSystemUsageRepository;

        public ItInterfaceUsageController(IGenericRepository<ItInterfaceUsage> repository, IItSystemUsageRepository itSystemUsageRepository)
        : base()
        {
            _repository = repository;
            _itSystemUsageRepository = itSystemUsageRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceUsageDTO>>))]
        public HttpResponseMessage GetByContract(int contractId)
        {
            try
            {
                var items = _repository.Get(x => x.ItContractId == contractId);
                var dtos = Map<IEnumerable<ItInterfaceUsage>, IEnumerable<ItInterfaceUsageDTO>>(items.Where(item => AllowRead(item.ItSystemUsage)));
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceUsageDTO>>))]
        public HttpResponseMessage Get(int usageId, int sysId, int interfaceId)
        {
            try
            {
                var key = ItInterfaceUsage.GetKey(usageId, sysId, interfaceId);
                var item = _repository.GetByKey(key);
                if (!AllowRead(item.ItSystemUsage))
                {
                    return Forbidden();
                }
                var dto = Map<ItInterfaceUsage, ItInterfaceUsageDTO>(item);
                return Ok(dto);
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
                var itSystemUsage = _itSystemUsageRepository.GetSystemUsage(usageId);
                if (itSystemUsage == null)
                {
                    return BadRequest("Unknown system usage id");
                }

                if (!AllowModify(itSystemUsage))
                {
                    return Forbidden();
                }

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
