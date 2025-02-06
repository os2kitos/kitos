using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Request.User;

namespace Presentation.Web.Controllers.API.V2.Internal.Users.Mapping
{
    public static class OrganizationRoleMappingExtensions
    {
        private static readonly EnumMap<OrganizationRoleChoice, OrganizationRole> Mapping;

        static OrganizationRoleMappingExtensions()
        {
            Mapping = new EnumMap<OrganizationRoleChoice, OrganizationRole>
            (
                (OrganizationRoleChoice.User, OrganizationRole.User),
                (OrganizationRoleChoice.LocalAdmin, OrganizationRole.LocalAdmin),
                (OrganizationRoleChoice.OrganizationModuleAdmin, OrganizationRole.OrganizationModuleAdmin),
                (OrganizationRoleChoice.SystemModuleAdmin, OrganizationRole.SystemModuleAdmin),
                (OrganizationRoleChoice.ContractModuleAdmin, OrganizationRole.ContractModuleAdmin),
                (OrganizationRoleChoice.GlobalAdmin, OrganizationRole.GlobalAdmin),
                (OrganizationRoleChoice.RightsHolderAccess, OrganizationRole.RightsHolderAccess)
            );
        }

        public static OrganizationRole ToOrganizationRole(this OrganizationRoleChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static OrganizationRoleChoice ToOrganizationRoleChoice(this OrganizationRole value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}