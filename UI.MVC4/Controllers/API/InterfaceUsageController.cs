using System;
using System.Collections.Generic;
using System.Net.Http;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class InterfaceUsageController : BaseApiController
    {
        private readonly IGenericRepository<InterfaceUsage> _repository;

        public InterfaceUsageController(IGenericRepository<InterfaceUsage> repository) 
        {
            _repository = repository;
        }

        public HttpResponseMessage Get(int usageId, int sysId, int interfaceId)
        {
            try
            {
                var item = _repository.GetByKey(new object[] {usageId, sysId, interfaceId});
                var dto = Map<InterfaceUsage, InterfaceUsageDTO>(item);
                return Ok(dto);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetByContract(int contractId)
        {
            try
            {
                var items = _repository.Get(x => x.ItContractId == contractId);
                var dtos = Map<IEnumerable<InterfaceUsage>, IEnumerable<InterfaceUsageDTO>>(items);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Patches IsWishedFor or ItContractId only.
        /// If the entry doesn't exist it's created.
        /// </summary>
        /// <param name="usageId"></param>
        /// <param name="sysId"></param>
        /// <param name="obj"></param>
        /// <param name="interfaceId"></param>
        /// <returns></returns>
        public HttpResponseMessage PatchOrCreate(int usageId, int interfaceId, int sysId, JObject obj)
        {
            try
            {
                var key = new object[] { usageId, sysId, interfaceId };
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

                _repository.Save();
                var outDto = Map<InterfaceUsage, InterfaceUsageDTO>(item);
                return Ok(outDto);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
