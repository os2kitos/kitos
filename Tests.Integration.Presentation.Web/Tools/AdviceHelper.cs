using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class AdviceHelper
    { 
        public static async Task<HttpResponseMessage> PostAdviceAsync(Core.DomainModel.Advice.Advice advice, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var recipients = advice.Reciepients.Select(x => new { Name = x.Name, RecpientType = x.RecpientType.ToString(), RecieverType = x.RecieverType.ToString() });

            var body = new
            {
                Scheduling = advice.Scheduling.Value.ToString(),
                advice.Subject,
                advice.Body,
                AlarmDate = advice.AlarmDate.Value.ToString(HttpApi.OdataDateTimeFormat),
                Reciepients = recipients
            };

            return await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"odata/advice?organizationId={organizationId}"), cookie, body);
        }

        public static async Task<HttpResponseMessage> GetContractAdvicesAsync(int contractId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"/Odata/advice?$filter=type eq '0' and RelationId eq {contractId}"), cookie);
        }
    }
}
