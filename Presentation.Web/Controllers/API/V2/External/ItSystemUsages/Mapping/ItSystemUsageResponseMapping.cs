using System;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Controllers.API.V2.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    //TODO: Unit test this beast
    public static class ItSystemUsageResponseMapping
    {
        public static ItSystemUsageResponseDTO MapSystemUsageDTO(ItSystemUsage systemUsage)
        {
            return new()
            {
                Uuid = systemUsage.Uuid,
                SystemContext = systemUsage.ItSystem.MapIdentityNamePairDTO(),
                OrganizationContext = systemUsage.Organization.MapShallowOrganizationResponseDTO(),
                CreatedBy = systemUsage.ObjectOwner.MapIdentityNamePairDTO(),
                LastModified = systemUsage.LastChanged,
                LastModifiedBy = systemUsage.LastChangedByUser.MapIdentityNamePairDTO(),
                General = new()
                {
                    LocalCallName = systemUsage.LocalCallName,
                    LocalSystemId = systemUsage.LocalSystemId,
                    Notes = systemUsage.Note,
                    MainContract = systemUsage.MainContract?.ItContract?.MapIdentityNamePairDTO(),
                    DataClassification = systemUsage.ItSystemCategories?.MapIdentityNamePairDTO(),
                    AssociatedProjects = systemUsage.ItProjects.Select(project => project.MapIdentityNamePairDTO()).ToList(),
                    NumberOfExpectedUsers = MapExpectedUsers(systemUsage),
                    SystemVersion = systemUsage.Version,
                    Validity = new ItSystemUsageValidityResponseDTO
                    {
                        EnforcedValid = systemUsage.Active,
                        Valid = systemUsage.IsActive,
                        ValidFrom = systemUsage.Concluded,
                        ValidTo = systemUsage.ExpirationDate
                    }
                },
                Roles = systemUsage.Rights.Select(ToRoleResponseDTO).ToList(),
                LocalKLEDeviations = new LocalKLEDeviationsResponseDTO
                {
                    AddedKLE = systemUsage.TaskRefs.Select(taskRef => taskRef.MapIdentityNamePairDTO()).ToList(),
                    RemovedKLE = systemUsage.TaskRefsOptOut.Select(taskRef => taskRef.MapIdentityNamePairDTO()).ToList()
                },
                OrganizationUsage = new OrganizationUsageResponseDTO
                {
                    ResponsibleOrganizationUnit = systemUsage.ResponsibleUsage?.OrganizationUnit?.MapIdentityNamePairDTO(),
                    UsingOrganizationUnits = systemUsage.UsedBy.Select(x => x.OrganizationUnit.MapIdentityNamePairDTO()).ToList()
                },
                ExternalReferences = systemUsage.ExternalReferences.Select(reference => MapExternalReferenceDTO(systemUsage, reference)).ToList(),
                OutgoingSystemRelations = systemUsage.UsageRelations.Select(MapSystemRelationDTO).ToList(),
                Archiving = new ArchivingRegistrationsResponseDTO(), //TODO
                GDPR = new GDPRRegistrationsResponseDTO()//TODO
            };
        }

        public static SystemRelationResponseDTO MapSystemRelationDTO(SystemRelation arg)
        {
            return new()
            {
                Uuid = arg.Uuid,
                Description = arg.Description,
                UrlReference = arg.Reference,
                AssociatedContract = arg.AssociatedContract?.MapIdentityNamePairDTO(),
                RelationFrequency = arg.UsageFrequency?.MapIdentityNamePairDTO(),
                UsingInterface = arg.RelationInterface?.MapIdentityNamePairDTO(),
                ToSystemUsage = arg.ToSystemUsage?.MapIdentityNamePairDTO()
            };
        }

        public static ExternalReferenceDataDTO MapExternalReferenceDTO(ItSystemUsage systemUsage, ExternalReference reference)
        {
            return new()
            {
                DocumentId = reference.ExternalReferenceId,
                Title = reference.Title,
                Url = reference.URL,
                MasterReference = systemUsage.Reference?.Id.Equals(reference.Id) == true
            };
        }

        public static RoleAssignmentResponseDTO ToRoleResponseDTO(ItSystemRight right)
        {
            return new() { Role = right.Role.MapIdentityNamePairDTO(), User = right.User.MapIdentityNamePairDTO() };
        }

        public static ExpectedUsersIntervalDTO MapExpectedUsers(ItSystemUsage systemUsage)
        {
            return systemUsage.UserCount switch
            {
                UserCount.BELOWTEN => new ExpectedUsersIntervalDTO { LowerBound = 0, UpperBound = 9 },
                UserCount.TENTOFIFTY => new ExpectedUsersIntervalDTO { LowerBound = 10, UpperBound = 50 },
                UserCount.FIFTYTOHUNDRED => new ExpectedUsersIntervalDTO { LowerBound = 50, UpperBound = 100 },
                UserCount.HUNDREDPLUS => new ExpectedUsersIntervalDTO { LowerBound = 100 },
                _ => throw new ArgumentOutOfRangeException(nameof(systemUsage.UserCount))
            };
        }
    }
}