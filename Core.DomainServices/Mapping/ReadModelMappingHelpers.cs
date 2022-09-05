using Core.Abstractions.Extensions;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices.Mapping
{
    public static class ReadModelMappingHelpers
    {
        public static string MapItSystemName(this ItSystemUsage systemUsage)
        {
            return systemUsage.ItSystem.FromNullable().Select(system => system.MapItSystemName()).GetValueOrDefault();
        }

        public static string MapItSystemName(this ItSystem system)
        {
            return $"{system.Name}{(system.Disabled ? " (Ikke aktivt)" : "")}";
        }
    }
}
