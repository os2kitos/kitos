using System;
using System.Net.Http;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Newtonsoft.Json.Linq;

namespace UI.MVC4.Controllers.API
{
    public class DataRowUsageController : BaseApiController
    {
        private readonly IGenericRepository<DataRowUsage> _repository;

        public DataRowUsageController(IGenericRepository<DataRowUsage> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Patches FrequencyId, Amount, Economy and Price only.
        /// If the entry doesn't exist it's created.
        /// </summary>
        /// <param name="rowId"></param>
        /// <param name="usageId"></param>
        /// <param name="sysId"></param>
        /// <param name="interfaceId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public HttpResponseMessage PatchOrCreate(int rowId, int usageId, int interfaceId, int sysId, JObject obj)
        {
            try
            {
                var key = new object[] { rowId, usageId, sysId, interfaceId };
                var item = _repository.GetByKey(key);
                // create if doesn't exists
                if (item == null)
                {
                    item = _repository.Create();
                    item.DataRowId = rowId;
                    item.ItSystemUsageId = usageId;
                    item.ItSystemId = sysId;
                    item.ItInterfaceId = interfaceId;

                    _repository.Insert(item);
                }

                var economyToken = obj.GetValue("economy");
                if (economyToken != null)
                    item.Economy = economyToken.Value<int?>();

                var priceToken = obj.GetValue("price");
                if (priceToken != null)
                    item.Price = priceToken.Value<int?>();

                var amountToken = obj.GetValue("amount");
                if (amountToken != null)
                    item.Amount = amountToken.Value<int?>();

                var freqToken = obj.GetValue("frequencyId");
                if (freqToken != null)
                    item.FrequencyId = freqToken.Value<int?>();

                _repository.Save();
                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
