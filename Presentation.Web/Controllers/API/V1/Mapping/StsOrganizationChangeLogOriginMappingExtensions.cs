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
        private static readonly IReadOnlyDictionary<StsOrganizationChangeLogOrigin, StsOrganizationChangeLogOriginOption> ApiToDataMap;
        private static readonly IReadOnlyDictionary<StsOrganizationChangeLogOriginOption, StsOrganizationChangeLogOrigin> DataToApiMap;

        static StsOrganizationChangeLogOriginMappingExtensions()
        {
            ApiToDataMap = new Dictionary<StsOrganizationChangeLogOrigin, StsOrganizationChangeLogOriginOption>
            {
                { StsOrganizationChangeLogOrigin.Background, StsOrganizationChangeLogOriginOption.Background},
                { StsOrganizationChangeLogOrigin.User, StsOrganizationChangeLogOriginOption.User },
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static StsOrganizationChangeLogOriginOption ToStsOrganizationChangeLogOriginOption(this StsOrganizationChangeLogOrigin value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static StsOrganizationChangeLogOrigin ToStsOrganizationChangeLogOrigin(this StsOrganizationChangeLogOriginOption value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}