using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Filters;
using Newtonsoft.Json.Converters;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class V2StyleJsonResponseSerialization : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            
            //TODO: Make sure it is not mutating the global context but just the request context
            if (actionExecutedContext.Response.Content is ObjectContent { Formatter: BaseJsonMediaTypeFormatter jsonFormatter })
            {
                jsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            }
        }
    }
}