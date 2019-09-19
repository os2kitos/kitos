using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Presentation.Web.Models;

namespace Tests.Unit.Presentation.Web.Helpers
{
    public static class ResponseMessageHelper
    {
        public static async Task<ApiReturnDTO<T>> ReadApiResponse<T>(this HttpResponseMessage response)
        {
            var responseAsJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<ApiReturnDTO<T>>(responseAsJson);
        }
    }
}
