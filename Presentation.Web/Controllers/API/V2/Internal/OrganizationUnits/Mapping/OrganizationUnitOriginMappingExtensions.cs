using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Types.Organization;

namespace Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits.Mapping
{
    public static class OrganizationUnitOriginMappingExtensions
    {
        private static readonly EnumMap<OrganizationUnitOriginChoice, OrganizationUnitOrigin> Mapping;

        static OrganizationUnitOriginMappingExtensions()
        {
            Mapping = new EnumMap<OrganizationUnitOriginChoice, OrganizationUnitOrigin>
            (
                (OrganizationUnitOriginChoice.Kitos, OrganizationUnitOrigin.Kitos),
                (OrganizationUnitOriginChoice.STSOrganisation, OrganizationUnitOrigin.STS_Organisation)
            );
        }

        public static OrganizationUnitOrigin ToOrganizationUnitOrigin(this OrganizationUnitOriginChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static OrganizationUnitOriginChoice ToOrganizationUnitOriginChoice(this OrganizationUnitOrigin value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}