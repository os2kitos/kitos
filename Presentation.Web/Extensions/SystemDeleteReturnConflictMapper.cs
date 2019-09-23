using System.Collections.Generic;
using Core.ApplicationServices.Model.System;
using Presentation.Web.Models.ItSystem;

namespace Presentation.Web.Extensions
{
    public static class SystemDeleteReturnConflictMapper
    {
        private static readonly Dictionary<DeleteResult, SystemDeleteConflict> MapToConflictDictionary = new Dictionary<DeleteResult, SystemDeleteConflict>()
        {
            { DeleteResult.InUse, SystemDeleteConflict.InUse},
            { DeleteResult.HasChildren, SystemDeleteConflict.HasChildren },
            { DeleteResult.HasInterfaceExhibits, SystemDeleteConflict.HasInterfaceExhibits }
        };

        public static SystemDeleteConflict MapToConflict(this DeleteResult input)
        {
            return MapToConflictDictionary[input];
        }
    }
}