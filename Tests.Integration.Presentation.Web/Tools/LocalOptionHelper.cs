using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class LocalOptionHelper
    {
        public static class ResourceNames
        {
            public const string LocalCriticalityTypes = "LocalCriticalityTypes";
            public const string LocalItContractTypes = "LocalItContractTypes";
            public const string LocalItContractTemplateTypes = "LocalItContractTemplateTypes";
            public const string LocalPurchaseFormTypes = "LocalPurchaseFormTypes";
            public const string LocalProcurementStrategyTypes = "LocalProcurementStrategyTypes";
            public const string LocalPaymentModelTypes = "LocalPaymentModelTypes";
            public const string LocalOptionExtendTypes = "LocalOptionExtendTypes";
            public const string LocalPaymentFrequencyTypes = "LocalPaymentFrequencyTypes";
            public const string LocalTerminationDeadlineTypes = "LocalTerminationDeadlineTypes";
        }

        public static async Task<List<OptionEntity<T>>> GetOptionsAsync<T>(string resource, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"odata/{resource}?organizationId={organizationId}");

            using var response = await HttpApi.GetWithCookieAsync(url, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return (await response.ReadOdataListResponseBodyAsAsync<OptionStub<T>>()).Cast<OptionEntity<T>>().ToList();
        }

        /// <summary>
        /// Simple stub required to cover the different local option types in order to allow deserializing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class OptionStub<T> : OptionEntity<T>
        {

        }
    }
}
