using System;
using System.Net.Http;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
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
                var key = new object[] {rowId, usageId, sysId, interfaceId};
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
                    try
                    {
                        item.Economy = economyToken.Value<int?>(); // throws error on empty string :(
                    }
                    catch
                    {
                        // if above fails it's a null-ish value
                        item.Economy = null;
                    }

                var priceToken = obj.GetValue("price");
                if (priceToken != null)
                    try
                    {
                        item.Price = priceToken.Value<int?>(); // throws error on empty string :(
                    }
                    catch
                    {
                        // if above fails it's a null-ish value
                        item.Price = null;
                    }


                var amountToken = obj.GetValue("amount");
                if (amountToken != null)
                    try
                    {
                        item.Amount = amountToken.Value<int?>(); // throws error on empty string :(
                    }
                    catch
                    {
                        // if above fails it's a null-ish value
                        item.Amount = null;
                    }


                var freqToken = obj.GetValue("frequencyId");
                if (freqToken != null)
                    try
                    {
                        item.FrequencyId = freqToken.Value<int?>(); // throws error on empty string :(
                    }
                    catch
                    {
                        // if above fails it's a null-ish value
                        item.FrequencyId = null;
                    }

                _repository.Save();
                return Ok();
            }
            catch (Exception e)
            {

                return LogError(e);
            }
        }
    }
}
