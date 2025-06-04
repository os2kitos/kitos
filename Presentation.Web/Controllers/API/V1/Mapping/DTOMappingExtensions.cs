using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1.Mapping
{
    public static class DTOMappingExtensions
    {
        public static NamedEntityDTO MapToNamedEntityDTO(this User source)
        {
            return new NamedEntityDTO(source.Id, source.GetFullName().TrimEnd());
        }

        public static UserWithEmailDTO MapToUserWithEmailDTO(this User source)
        {
            return new UserWithEmailDTO(source.Id, source.GetFullName(), source.Email);
        }

        public static NamedEntityWithEnabledStatusDTO MapToNamedEntityWithEnabledStatusDTO(this ItSystemUsage source)
        {
            return new NamedEntityWithEnabledStatusDTO(source.Id, source.ItSystem.Name, source.ItSystem.Disabled);
        }

        public static ShallowOrganizationDTO MapToShallowOrganizationDTO(this Organization source)
        {
            return new ShallowOrganizationDTO(source.Id, source.Name) { CvrNumber = source.GetActiveCvr() };
        }
    }
}