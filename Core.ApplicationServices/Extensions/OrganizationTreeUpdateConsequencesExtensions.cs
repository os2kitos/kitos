using Core.DomainModel.Organization;
using System.Collections.Generic;
using System.Linq;

namespace Core.ApplicationServices.Extensions
{
    public static class OrganizationTreeUpdateConsequencesExtensions
    {
        public static IEnumerable<StsOrganizationConsequenceLog> ConvertConsequencesToConsequenceLogs(this OrganizationTreeUpdateConsequences consequences)
        {
            var logs = new List<StsOrganizationConsequenceLog>();
            logs.AddRange(MapAddedOrganizationUnits(consequences));
            logs.AddRange(MapRenamedOrganizationUnits(consequences));
            logs.AddRange(MapMovedOrganizationUnits(consequences));
            logs.AddRange(MapRemovedOrganizationUnits(consequences));
            logs.AddRange(MapConvertedOrganizationUnits(consequences));

            return logs;
        }

        private static IEnumerable<StsOrganizationConsequenceLog> MapConvertedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .DeletedExternalUnitsBeingConvertedToNativeUnits
                .Select(converted => new StsOrganizationConsequenceLog
                {
                    Name = converted.organizationUnit.Name,
                    Type = ConnectionUpdateOrganizationUnitChangeType.Converted,
                    ExternalUnitUuid = converted.externalOriginUuid,
                    Description = $"'{converted.organizationUnit.Name}' er slettet i FK Organisation men konverteres til KITOS enhed, da den anvendes aktivt i KITOS."
                })
                .ToList();
        }

        private static IEnumerable<StsOrganizationConsequenceLog> MapRemovedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .DeletedExternalUnitsBeingDeleted
                .Select(deleted => new StsOrganizationConsequenceLog
                {
                    Name = deleted.organizationUnit.Name,
                    Type = ConnectionUpdateOrganizationUnitChangeType.Deleted,
                    ExternalUnitUuid = deleted.externalOriginUuid,
                    Description = $"'{deleted.organizationUnit.Name}' slettes."
                })
                .ToList();
        }

        private static IEnumerable<StsOrganizationConsequenceLog> MapMovedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .OrganizationUnitsBeingMoved
                .Select(moved =>
                {
                    var (movedUnit, oldParent, newParent) = moved;
                    return new StsOrganizationConsequenceLog
                    {
                        Name = movedUnit.Name,
                        Type = ConnectionUpdateOrganizationUnitChangeType.Moved,
                        ExternalUnitUuid = movedUnit.ExternalOriginUuid.GetValueOrDefault(),
                        Description = $"'{movedUnit.Name}' flyttes fra at være underenhed til '{oldParent.Name}' til fremover at være underenhed for {newParent.Name}"
                    };
                })
                .ToList();
        }

        private static IEnumerable<StsOrganizationConsequenceLog> MapRenamedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .OrganizationUnitsBeingRenamed
                .Select(renamed =>
                {
                    var (affectedUnit, oldName, newName) = renamed;
                    return new StsOrganizationConsequenceLog
                    {
                        Name = oldName,
                        Type = ConnectionUpdateOrganizationUnitChangeType.Renamed,
                        ExternalUnitUuid = affectedUnit.ExternalOriginUuid.GetValueOrDefault(),
                        Description = $"'{oldName}' omdøbes til '{newName}'"
                    };
                })
                .ToList();
        }

        private static IEnumerable<StsOrganizationConsequenceLog> MapAddedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .AddedExternalOrganizationUnits
                .Select(added => new StsOrganizationConsequenceLog
                {
                    Name = added.unitToAdd.Name,
                    Type = ConnectionUpdateOrganizationUnitChangeType.Added,
                    ExternalUnitUuid = added.unitToAdd.Uuid,
                    Description = $"'{added.unitToAdd.Name}' tilføjes som underenhed til '{added.parent?.Name}'"
                }
                )
                .ToList();
        }
    }
}
