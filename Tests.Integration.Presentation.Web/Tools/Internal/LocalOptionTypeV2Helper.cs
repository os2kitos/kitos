using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace Tests.Integration.Presentation.Web.Tools.Internal
{
    public static class LocalOptionTypeV2Helper
    {
        private const string LocalOptionTypesSuffix = "local-option-types";

        //Prefix types
        public const string ItSystemPrefix = "it-systems";
        public const string DataProcessingPrefix = "data-processing";
        public const string ItContractPrefix = "it-contracts";
        public const string OrganizationUnitPrefix = "organization-units";

        //It System types
        public const string ItSystemRoles = "it-systems-roles";
        public const string BusinessTypes = "business-types";
        public const string ArchiveTypes = "archive-types";
        public const string ArchiveLocationTypes = "archive-location-types";
        public const string ArchiveTestLocationTypes = "archive-test-location-types";
        public const string SensitivePersonalDataTypes = "sensitive-personal-data-types";
        public const string LocalRegisterTypes = "local-register-types";
        public const string ItSystemCategoriesTypes = "it-system-categories-types";
        public const string DataTypes = "data-types";
        public const string FrequencyRelationTypes = "frequency-relation-types";
        public const string InterfaceTypes = "interface-types";

        //It contract types
        public const string ItContractRoles = "it-contract-roles";
        public const string ItContractTypes = "it-contract-types";
        public const string TemplateTypes = "template-types";
        public const string PurchaseFormTypes = "purchase-form-ypes";
        public const string PaymentModelTypes = "payment-model-types";
        public const string AgreementElementTypes = "agreement-element-types";
        public const string OptionExtendTypes = "option-extend-types";
        public const string PaymentFrequencyTypes = "payment-frequency-types";
        public const string PriceRegulationTypes = "price-regulation-types";
        public const string ProcurementStrategyTypes = "procurement-strategy-types";
        public const string TerminationDeadlineTypes = "termination-deadline-types";
        public const string CriticalityTypes = "criticality-types";

        //Data processing types
        public const string DprRoles = "dpr-roles";
        public const string BasisForTransferTypes = "basis-for-transfer-types";
        public const string OversightOptionTypes = "oversight-option-types";
        public const string DataResponsibleTypes = "data-responsible-types";
        public const string CountryOptionTypes = "country-option-types";

        //Organization unit types
        public const string OrganizationUnitRoles = "organization-unit-roles";


        public static async Task<HttpResponseMessage> GetLocalOptionTypes(Guid organizationUuid, string choiceTypeName, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}"), cookie);
        }

        public static async Task<HttpResponseMessage> GetLocalOptionType(Guid organizationUuid, string choiceTypeName, Guid optionUuid, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}/{optionUuid}"), cookie);
        }

        public static async Task<HttpResponseMessage> CreateLocalOptionType(Guid organizationUuid, string choiceTypeName, LocalOptionCreateRequestDTO dto, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PostWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}"), cookie, dto);
        }

        public static async Task<HttpResponseMessage> PatchLocalOptionType(Guid organizationUuid, Guid optionUuid, string choiceTypeName, LocalRegularOptionUpdateRequestDTO dto, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}/{optionUuid}"), cookie, dto);
        }

        public static async Task<HttpResponseMessage> DeleteLocalOptionType(Guid organizationUuid, Guid optionUuid, string choiceTypeName, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.DeleteWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}/{optionUuid}"), cookie);
        }
    }
}
