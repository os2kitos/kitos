

using System.Net.Http;
using System.Threading.Tasks;
using System;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Tests.Integration.Presentation.Web.Tools.Internal.ItSystem
{
    public static class ItSystemInternalV2Helper
    {
        private const string ItSystemsApiPrefix = "api/v2/internal/it-systems";
        private const string LocalOptionTypesSuffix = "local-option-types";

        public static async Task<HttpResponseMessage> GetLocalOptionTypes(Guid organizationUuid, string choiceTypeName)
        {
            return await LocalOptionTypeV2Helper.GetLocalOptionTypes(organizationUuid, choiceTypeName, ItSystemsApiPrefix);

        }

        public static async Task<HttpResponseMessage> GetLocalOptionTypeByOptionId(Guid organizationUuid, string choiceTypeName, Guid optionUuid)
        {
            return await LocalOptionTypeV2Helper.GetLocalOptionType(organizationUuid, choiceTypeName, optionUuid,
                ItSystemsApiPrefix);
        }

        public static async Task<HttpResponseMessage> CreateLocalOptionType(Guid organizationUuid, string choiceTypeName, LocalRegularOptionCreateRequestDTO dto)
        {
            return await LocalOptionTypeV2Helper.CreateLocalOptionType(organizationUuid, choiceTypeName, dto,
                ItSystemsApiPrefix);
        }

        public static async Task<HttpResponseMessage> PatchLocalOptionType(Guid organizationUuid, Guid optionUuid, string choiceTypeName, LocalRegularOptionUpdateRequestDTO dto)
        {
            return await LocalOptionTypeV2Helper.PatchLocalOptionType(organizationUuid, optionUuid, choiceTypeName, dto,
                ItSystemsApiPrefix);
        }

        public static async Task<HttpResponseMessage> DeleteLocalOptionType(Guid organizationUuid, Guid optionUuid, string choiceTypeName)
        {
            return await LocalOptionTypeV2Helper.DeleteLocalOptionType(organizationUuid, optionUuid, choiceTypeName,
                ItSystemsApiPrefix);
        }
    }
}
