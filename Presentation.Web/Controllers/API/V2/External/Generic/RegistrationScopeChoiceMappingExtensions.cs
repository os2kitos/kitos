using Core.DomainModel;
using System.Collections.Generic;
using System;
using System.Linq;
using Core.Abstractions.Extensions;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public static class RegistrationScopeChoiceMappingExtensions
    {
        private static readonly IReadOnlyDictionary<RegistrationScopeChoice, AccessModifier> ApiToDataMap;
        private static readonly IReadOnlyDictionary<AccessModifier, RegistrationScopeChoice> DataToApiMap;

        static RegistrationScopeChoiceMappingExtensions()
        {
            ApiToDataMap = new Dictionary<RegistrationScopeChoice, AccessModifier>
            {
                { RegistrationScopeChoice.Global, AccessModifier.Public },
                { RegistrationScopeChoice.Local, AccessModifier.Local },
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static AccessModifier FromChoice(this RegistrationScopeChoice value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static RegistrationScopeChoice ToChoice(this AccessModifier value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}