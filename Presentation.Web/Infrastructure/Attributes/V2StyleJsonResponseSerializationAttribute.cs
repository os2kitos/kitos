using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Filters;
using Presentation.Web.Infrastructure.Config;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class V2StyleJsonResponseSerializationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            if (DisableV2StyleEnumSerialization(actionExecutedContext))
            {
                return;
            }
            if (actionExecutedContext.Response is { Content: ObjectContent { Formatter: JsonMediaTypeFormatter, Value: { } value } })
            {
                //Take a copy of the existing (global) media type formatter and extend that with the StringEnumConverter
                var updatedMediaTypeFormatter = V2JsonSerializationConfig.JsonMediaTypeFormatter;

                //Update the response content
                actionExecutedContext.Response.Content = new ObjectContent(value.GetType(), value, updatedMediaTypeFormatter, "application/json");
            }
        }

        public bool DisableV2StyleEnumSerialization(HttpActionExecutedContext context)
        {
            return context
                       .Request
                       .Headers
                       .TryGetValues(KitosConstants.Headers.SerializeEnumAsInteger, out var values) &&
                   values.Any(v => bool.TryParse(v, out var enabled) && enabled);
        }
    }
}