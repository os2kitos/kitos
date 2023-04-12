using Core.Abstractions.Types;
using Presentation.Web.Models.API.V2.Types.System;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public static class SystemDeletionConflictMappingExtensions
    {
        private static readonly EnumMap<SystemDeletionConflict, Core.ApplicationServices.Model.System.SystemDeletionConflict> Mapping;

        static SystemDeletionConflictMappingExtensions()
        {
            Mapping = new EnumMap<SystemDeletionConflict, Core.ApplicationServices.Model.System.SystemDeletionConflict>
            (
                (SystemDeletionConflict.HasChildSystems, Core.ApplicationServices.Model.System.SystemDeletionConflict.HasChildren),
                (SystemDeletionConflict.HasInterfaceExposures, Core.ApplicationServices.Model.System.SystemDeletionConflict.HasInterfaceExhibits),
                (SystemDeletionConflict.HasItSystemUsages, Core.ApplicationServices.Model.System.SystemDeletionConflict.InUse)
            );
        }

        public static Core.ApplicationServices.Model.System.SystemDeletionConflict FromChoice(this SystemDeletionConflict value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static SystemDeletionConflict ToChoice(this Core.ApplicationServices.Model.System.SystemDeletionConflict value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}