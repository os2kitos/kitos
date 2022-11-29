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
        private static readonly IReadOnlyDictionary<ExternalOrganizationChangeLogResponsible, StsOrganizationChangeLogOriginOption> ApiToDataMap;
        private static readonly IReadOnlyDictionary<StsOrganizationChangeLogOriginOption, ExternalOrganizationChangeLogResponsible> DataToApiMap;

        static StsOrganizationChangeLogOriginMappingExtensions()
        {
            ApiToDataMap = new Dictionary<ExternalOrganizationChangeLogResponsible, StsOrganizationChangeLogOriginOption>
            {
                { ExternalOrganizationChangeLogResponsible.Background, StsOrganizationChangeLogOriginOption.Background},
                { ExternalOrganizationChangeLogResponsible.User, StsOrganizationChangeLogOriginOption.User },
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static StsOrganizationChangeLogOriginOption ToStsOrganizationChangeLogOriginOption(this ExternalOrganizationChangeLogResponsible value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }
    }
}