using System;
using System.Collections.Generic;
using System.Net.Http;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

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

        public HttpResponseMessage GetSingle(int usageId, int exhibitId)
        {
            try
            {
                var item = _repository.GetByKey(new object[] { usageId, exhibitId });
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

        public HttpResponseMessage Post(int usageId, int exhibitId, ItInterfaceExhibitUsageDTO dto)
        {
            try
            {
                var item = _repository.GetByKey(new object[] { usageId, exhibitId });
                if (item != null)
                    return Conflict("Already exists");

                var entity = Map<ItInterfaceExhibitUsageDTO, ItInterfaceExhibitUsage>(dto);

                _repository.Insert(entity);
                _repository.Save();

                return Ok(entity);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage Delete(int usageId, int exhibitId)
        {
            try
            {
                var key = new object[] { usageId, exhibitId };
                var item = _repository.GetByKey(key);
                if (item == null)
                    return NotFound();

                _repository.DeleteByKey(key);
                _repository.Save();

                return Ok();
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
                var key = new object[] { usageId, exhibitId };
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
