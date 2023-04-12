using Core.Abstractions.Types;
using Presentation.Web.Models.API.V2.Types.Interface;

namespace Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping
{
    public static class ItInterfaceDeletionConflictMappingExtensions
    {
        private static readonly EnumMap<ItInterfaceDeletionConflict, Core.ApplicationServices.Model.Interface.ItInterfaceDeletionConflict> Mapping;

        static ItInterfaceDeletionConflictMappingExtensions()
        {
            Mapping = new EnumMap<ItInterfaceDeletionConflict, Core.ApplicationServices.Model.Interface.ItInterfaceDeletionConflict>
            (
                (ItInterfaceDeletionConflict.ExposedByItSystem, Core.ApplicationServices.Model.Interface.ItInterfaceDeletionConflict.ExposedByItSystem)
            );
        }

        public static Core.ApplicationServices.Model.Interface.ItInterfaceDeletionConflict FromChoice(this ItInterfaceDeletionConflict value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static ItInterfaceDeletionConflict ToChoice(this Core.ApplicationServices.Model.Interface.ItInterfaceDeletionConflict value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}