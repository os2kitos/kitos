using Core.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class GDPRPersonalDataMappingExtensions
    {

        private static readonly IReadOnlyDictionary<GDPRPersonalDataChoice, GDPRPersonalDataOption> ApiToDataMap;
        private static readonly IReadOnlyDictionary<GDPRPersonalDataOption, GDPRPersonalDataChoice> DataToApiMap;

        static GDPRPersonalDataMappingExtensions()
        {
            ApiToDataMap = new Dictionary<GDPRPersonalDataChoice, GDPRPersonalDataOption>
            {
                { GDPRPersonalDataChoice.CprNumber, GDPRPersonalDataOption.CprNumber},
                { GDPRPersonalDataChoice.OtherPrivateMatters, GDPRPersonalDataOption.OtherPrivateMatters},
                { GDPRPersonalDataChoice.SocialProblems, GDPRPersonalDataOption.SocialProblems}
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static GDPRPersonalDataOption ToGDPRPersonalDataOption(this GDPRPersonalDataChoice value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static GDPRPersonalDataChoice ToGDPRPersonalDataChoice(this GDPRPersonalDataOption value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }
    }
}