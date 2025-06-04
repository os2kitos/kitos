using Core.DomainModel.Organization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Options;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class OptionV2ApiHelper
    {
        public static class ResourceName
        {
            public const string BusinessType = "business-types";
            public const string ItSystemUsageDataClassification = "it-system-usage-data-classification-types";
            public const string ItSystemUsageRelationFrequencies = "it-system-usage-relation-frequency-types";
            public const string ItSystemUsageArchiveTypes = "it-system-usage-archive-types";
            public const string ItSystemUsageRegisterTypes = "it-system-usage-registered-data-category-types";
            public const string ItSystemSensitivePersonalDataTypes = "it-system-usage-sensitive-personal-data-types";
            public const string ItSystemUsageArchiveTestLocations = "it-system-usage-archive-test-location-types";
            public const string ItSystemUsageArchiveLocations = "it-system-usage-archive-location-types";
            public const string ItSystemUsageRoles = "it-system-usage-role-types";

            public const string ItContractContractTypes = "it-contract-contract-types";
            public const string ItContractContractTemplateTypes = "it-contract-contract-template-types";
            public const string ItContractPurchaseTypes = "it-contract-purchase-types";
            public const string ItContractPaymentModelTypes = "it-contract-payment-model-types";
            public const string ItContractAgreementElementTypes = "it-contract-agreement-element-types";
            public const string ItContractPaymentFrequencyTypes = "it-contract-payment-frequency-types";
            public const string ItContractPriceRegulationTypes = "it-contract-price-regulation-types";
            public const string ItContractProcurementStrategyTypes = "it-contract-procurement-strategy-types";
            public const string ItContractAgreementExtensionOptionTypes = "it-contract-agreement-extension-option-types";
            public const string ItContractNoticePeriodMonthTypes = "it-contract-notice-period-month-types";
            public const string ItContractRoles = "it-contract-role-types";
            public const string CriticalityTypes = "it-contract-criticality-types";

            public const string DataProcessingRegistrationDataResponsible = "data-processing-registration-data-responsible-types";
            public const string DataProcessingRegistrationBasisForTransfer = "data-processing-registration-basis-for-transfer-types";
            public const string DataProcessingRegistrationCountry = "data-processing-registration-country-types";
            public const string DataProcessingRegistrationOversight = "data-processing-registration-oversight-types";
            public const string DataProcessingRegistrationRoles = "data-processing-registration-role-types";

            public const string ItInterfaceTypes = "it-interface-interface-types";
            public const string ItInterfaceDataTypes = "it-interface-interface-data-types";

            public const string OrganizationUnitTypes = "organization-unit-role-types";

        }

        public static async Task<IEnumerable<IdentityNamePairResponseDTO>> GetOptionsAsync(string resource, Guid orgUuid, int pageSize, int pageNumber)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v2/{resource}?organizationUuid={orgUuid}&pageSize={pageSize}&page={pageNumber}");

            using var response = await HttpApi.GetWithTokenAsync(url, token.Token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<IdentityNamePairResponseDTO>>();
        }

        public static async Task<RegularOptionExtendedResponseDTO> GetOptionAsync(string resource, Guid optionTypeUuid, Guid organizationUuid)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v2/{resource}/{optionTypeUuid}?organizationUuid={organizationUuid}");
            
            using var response = await HttpApi.GetWithTokenAsync(url, token.Token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<RegularOptionExtendedResponseDTO>();
        }

        public static async Task<RoleOptionExtendedResponseDTO> GetRoleOptionAsync(string resource, Guid optionTypeUuid, Guid organizationUuid)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v2/{resource}/{optionTypeUuid}?organizationUuid={organizationUuid}");

            using var response = await HttpApi.GetWithTokenAsync(url, token.Token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<RoleOptionExtendedResponseDTO>();
        }

        public static async Task<IdentityNamePairResponseDTO> GetRandomOptionAsync(string resource, Guid organizationUuid)
        {
            var options = await GetOptionsAsync(resource, organizationUuid, 25, 0);
            return options.RandomItem();
        }
    }
}
