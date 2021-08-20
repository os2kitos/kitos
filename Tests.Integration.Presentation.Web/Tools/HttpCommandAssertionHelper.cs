using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class HttpCommandAssertionHelper
    {
        public static async Task<HttpResponseMessage> WithExpectedResponseCode(this Task<HttpResponseMessage> responseMessage, HttpStatusCode expectedStatusCode)
        {
            var httpResponseMessage = await responseMessage;
            Assert.Equal(expectedStatusCode,httpResponseMessage.StatusCode);
            return httpResponseMessage;
        }
    }
}
