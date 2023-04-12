using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1.Mapping
{
    public static class DTOMappingExtensions
    {
        public static IEnumerable<NamedEntityDTO> MapToNamedEntityDTOs<T>(this IEnumerable<T> source)
            where T : IHasId, IHasName
        {
            return source.Select(MapToNamedEntityDTO);
        }

        public static NamedEntityDTO MapToNamedEntityDTO<T>(this T source)
            where T : IHasId, IHasName
        {
            return new NamedEntityDTO(source.Id, source.Name);
        }

        public static EntityWithOrganizationRelationshipDTO MapToEntityWithOrganizationRelationshipDTO<T>(this T source)
            where T : IHasId, IHasName, IOwnedByOrganization
        {
            return new EntityWithOrganizationRelationshipDTO(source.Id, source.Name, source.Organization?.MapToShallowOrganizationDTO());
        }

        public static NamedEntityWithUuidDTO MapToNamedEntityDTO(this NamedEntityWithUuid source)
        {
            return new NamedEntityWithUuidDTO(source.Id, source.Name, source.Uuid);
        }

        public static NamedEntityDTO MapToNamedEntityDTO(this NamedEntity source)
        {
            return new NamedEntityDTO(source.Id, source.Name);
        }

        public static NamedEntityDTO MapToNamedEntityDTO(this User source)
        {
            return new NamedEntityDTO(source.Id, source.GetFullName().TrimEnd());
        }

        public static UserWithEmailDTO MapToUserWithEmailDTO(this User source)
        {
            return new UserWithEmailDTO(source.Id, source.GetFullName(), source.Email);
        }

        public static NamedEntityWithEnabledStatusDTO MapToNamedEntityWithEnabledStatusDTO(this ItSystem source)
        {
            return new NamedEntityWithEnabledStatusDTO(source.Id, source.Name, source.Disabled);
        }

        public static NamedEntityWithEnabledStatusDTO MapToNamedEntityWithEnabledStatusDTO(this ItSystemUsage source)
        {
            return new NamedEntityWithEnabledStatusDTO(source.Id, source.ItSystem.Name, source.ItSystem.Disabled);
        }

        public static ShallowOrganizationDTO MapToShallowOrganizationDTO(this Organization source)
        {
            return new ShallowOrganizationDTO(source.Id, source.Name) { CvrNumber = source.GetActiveCvr() };
        }

        public static IEnumerable<ShallowOrganizationDTO> MapToShallowOrganizationDTOs(this IEnumerable<Organization> source)
        {
            return source.Select(MapToShallowOrganizationDTO);
        }
    }
}