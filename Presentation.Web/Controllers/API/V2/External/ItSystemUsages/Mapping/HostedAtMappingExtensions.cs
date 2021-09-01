using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.Result;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class HostedAtMappingExtensions
    {
        private static readonly IReadOnlyDictionary<HostingChoice, HostedAt> ApiToDataMap;
        private static readonly IReadOnlyDictionary<HostedAt, HostingChoice> DataToApiMap;

        static HostedAtMappingExtensions()
        {
            ApiToDataMap = new Dictionary<HostingChoice, HostedAt>
            {
                { HostingChoice.External, HostedAt.EXTERNAL },
                { HostingChoice.OnPremise, HostedAt.ONPREMISE },
                { HostingChoice.Undecided, HostedAt.UNDECIDED }
            }.AsReadOnly();
            
            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static HostedAt ToHostedAt(this HostingChoice value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static HostingChoice ToHostingChoice(this HostedAt value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}