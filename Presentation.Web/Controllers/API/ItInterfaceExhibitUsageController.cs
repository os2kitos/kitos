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
    public class ItInterfaceExhibitUsageController : BaseApiController
    {
        private readonly IGenericRepository<ItInterfaceExhibitUsage> _repository;

        public ItInterfaceExhibitUsageController(IGenericRepository<ItInterfaceExhibitUsage> repository)
        {
            _repository = repository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceExhibitUsageDTO>>))]
        public HttpResponseMessage GetByContract(int contractId)
        {
            try
            {
                var items = _repository.Get(x => x.ItContractId == contractId);
                var dto = Map<IEnumerable<ItInterfaceExhibitUsage>, IEnumerable<ItInterfaceExhibitUsageDTO>>(items);

                return Ok(dto);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<ItInterfaceExhibitUsageDTO>))]
        public HttpResponseMessage GetSingle(int usageId, int exhibitId)
        {
            try
            {
                var key = ItInterfaceExhibitUsage.GetKey(usageId, exhibitId);
                var item = _repository.GetByKey(key);
                if (item == null)
                    return NotFound();

                var dto = Map<ItInterfaceExhibitUsage, ItInterfaceExhibitUsageDTO>(item);

                return Ok(dto);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Patches IsWishedFor or ItContractId only.
        /// If the entry doesn't exist it's created.
        /// </summary>
        /// <param name="usageId"></param>
        /// <param name="exhibitId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public HttpResponseMessage PatchOrCreate(int usageId, int exhibitId, JObject obj)
        {
            try
            {
                var key = ItInterfaceExhibitUsage.GetKey(usageId, exhibitId);
                var item = _repository.GetByKey(key);
                // create if doesn't exists
                if (item == null)
                {
                    item = _repository.Create();
                    item.ItSystemUsageId = usageId;
                    item.ItInterfaceExhibitId = exhibitId;

                    _repository.Insert(item);
                }

                var wishToken = obj.GetValue("isWishedFor");
                if (wishToken != null)
                    item.IsWishedFor = wishToken.Value<bool>();

                var contractToken = obj.GetValue("itContractId");
                if (contractToken != null)
                    item.ItContractId = contractToken.Value<int?>();

                _repository.Save();
                var outDto = Map<ItInterfaceExhibitUsage, ItInterfaceExhibitUsageDTO>(item);
                return Ok(outDto);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
