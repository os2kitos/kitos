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

            var recipients = advice.Reciepients.Select(x => new
            {
                Name = x.Name, 
                RecpientType = x.RecpientType.ToString(), 
                RecieverType = x.RecieverType.ToString()
            }).ToList();

            var body = new
            {
                RelationId = advice.RelationId,
                Type = advice.Type?.ToString("G"),
                Scheduling = advice.Scheduling?.ToString(),
                Subject = advice.Subject,
                Body = advice.Body,
                AlarmDate = advice.AlarmDate?.ToString(HttpApi.OdataDateTimeFormat),
                Reciepients = recipients,
                AdviceType = advice.AdviceType.ToString("D"),
                StopDate = advice.StopDate?.ToString(HttpApi.OdataDateTimeFormat)
            };

            return await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"odata/advice?organizationId={organizationId}"), cookie, body);
        }

        public static async Task<HttpResponseMessage> DeleteAdviceAsync(int adviceId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"odata/advice({adviceId})"), cookie);
        }
        public static async Task<HttpResponseMessage> DeactivateAdviceAsync(int adviceId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"odata/DeactivateAdvice?key={adviceId}"), cookie, null);
        }

        public static async Task<HttpResponseMessage> GetContractAdvicesAsync(int contractId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"/Odata/advice?$filter=type eq '0' and RelationId eq {contractId}"), cookie);
        }
    }
}
