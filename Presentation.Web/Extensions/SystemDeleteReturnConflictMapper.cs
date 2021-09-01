using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.System;
using Presentation.Web.Models.API.V1.ItSystem;

namespace Presentation.Web.Extensions
{
    public static class SystemDeleteReturnConflictMapper
    {
        private static readonly IReadOnlyDictionary<SystemDeleteResult, SystemDeleteConflict> MapToConflictDictionary = new Dictionary<SystemDeleteResult, SystemDeleteConflict>()
        {
            { SystemDeleteResult.InUse, SystemDeleteConflict.InUse},
            { SystemDeleteResult.HasChildren, SystemDeleteConflict.HasChildren },
            { SystemDeleteResult.HasInterfaceExhibits, SystemDeleteConflict.HasInterfaceExhibits }
        };

        public static SystemDeleteConflict MapToConflict(this SystemDeleteResult input)
        {
            if (MapToConflictDictionary.TryGetValue(input, out var mappedValue))
            {
                return mappedValue;
            }
            throw new NotSupportedException($"No mapping exists for: {nameof(input)}");
        }
    }
}