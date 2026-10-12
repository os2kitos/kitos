using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class V2StyleJsonResponseSerializationAttribute : ActionFilterAttribute
    {
        private static readonly JsonSerializerSettings V2Settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Converters = { new StringEnumConverter() }
        };

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            if (DisableV2StyleEnumSerialization(context))
                return;

            if (context.Result is ObjectResult objectResult)
            {
                context.Result = new JsonResult(objectResult.Value, V2Settings)
                {
                    StatusCode = objectResult.StatusCode
                };
            }
        }

        private static bool DisableV2StyleEnumSerialization(ActionExecutedContext context)
        {
            return context.HttpContext.Request.Headers
                .TryGetValue(KitosConstants.Headers.SerializeEnumAsInteger, out var values) &&
                values.Any(v => bool.TryParse(v, out var enabled) && enabled);
        }
    }
}
