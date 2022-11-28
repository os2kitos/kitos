using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1.Organizations;

namespace Presentation.Web.Controllers.API.V1.Mapping
{
    public static class StsOrganizationChangeLogOriginMappingExtensions
    {
        private static readonly IReadOnlyDictionary<ExternalOrganizationChangeLogOrigin, StsOrganizationChangeLogOriginOption> ApiToDataMap;
        private static readonly IReadOnlyDictionary<StsOrganizationChangeLogOriginOption, ExternalOrganizationChangeLogOrigin> DataToApiMap;

        static StsOrganizationChangeLogOriginMappingExtensions()
        {
            ApiToDataMap = new Dictionary<ExternalOrganizationChangeLogOrigin, StsOrganizationChangeLogOriginOption>
            {
                { ExternalOrganizationChangeLogOrigin.Background, StsOrganizationChangeLogOriginOption.Background},
                { ExternalOrganizationChangeLogOrigin.User, StsOrganizationChangeLogOriginOption.User },
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static StsOrganizationChangeLogOriginOption ToStsOrganizationChangeLogOriginOption(this ExternalOrganizationChangeLogOrigin value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }
    }
}