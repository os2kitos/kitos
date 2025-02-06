using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.UIConfiguration;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Presentation.Web.Models.API.V2.Response.Organization;
using ContactPerson = Core.DomainModel.ContactPerson;
using Organization = Core.DomainModel.Organization.Organization;


namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public class OrganizationResponseMapper : IOrganizationResponseMapper
    {
        private readonly IOrganizationTypeMapper _organizationTypeMapper;

        public OrganizationResponseMapper(IOrganizationTypeMapper organizationTypeMapper)
        {
            _organizationTypeMapper = organizationTypeMapper;
        }

        public UIRootConfigResponseDTO ToUIRootConfigDTO(Config config)
        {
            return new()
            {
                ShowDataProcessing = config.ShowDataProcessing,
                ShowItSystemModule = config.ShowItSystemModule,
                ShowItContractModule = config.ShowItContractModule
            };
        }

        public OrganizationResponseDTO ToOrganizationDTO(Organization organization)
        {
            return new(organization.Uuid, organization.Name, organization.GetActiveCvr(),
                _organizationTypeMapper.MapOrganizationType(organization.Type));
        }

        public OrganizationMasterDataRolesResponseDTO ToRolesDTO(OrganizationMasterDataRoles roles)
        {
            var contactPersonDto = ToContactPersonDTO(roles.ContactPerson);
            var dataResponsibleDto = ToDataResponsibleDTO(roles.DataResponsible);
            var dataProtectionAdvisor = ToDataProtectionAdvisorDTO(roles.DataProtectionAdvisor);
            return new OrganizationMasterDataRolesResponseDTO()
            {
                OrganizationUuid = roles.OrganizationUuid,
                ContactPerson = contactPersonDto,
                DataResponsible = dataResponsibleDto,
                DataProtectionAdvisor = dataProtectionAdvisor
            };
        }
        

        private static ContactPersonResponseDTO ToContactPersonDTO(ContactPerson contactPerson)
        {
            return new()
            {
                Email = contactPerson.Email,
                Id = contactPerson.Id,
                LastName = contactPerson.LastName,
                Name = contactPerson.Name,
                PhoneNumber = contactPerson.PhoneNumber,
            };
        }

        private static DataResponsibleResponseDTO ToDataResponsibleDTO(DataResponsible dataResponsible)
        {
            return new()
            {
                Email = dataResponsible.Email,
                Id = dataResponsible.Id,
                Cvr = dataResponsible.Cvr,
                Name = dataResponsible.Name,
                Address = dataResponsible.Adress,
                Phone = dataResponsible.Phone,
            };
        }

        private static DataProtectionAdvisorResponseDTO ToDataProtectionAdvisorDTO(
            DataProtectionAdvisor dataProtectionAdvisor)
        {
            return new()
            {
                Email = dataProtectionAdvisor.Email,
                Id = dataProtectionAdvisor.Id,
                Cvr = dataProtectionAdvisor.Cvr,
                Name = dataProtectionAdvisor.Name,
                Address = dataProtectionAdvisor.Adress,
                Phone = dataProtectionAdvisor.Phone,
            };
        }

        public OrganizationMasterDataResponseDTO ToMasterDataDTO(Organization organization)
        {
            return new OrganizationMasterDataResponseDTO()
            {
                Cvr = organization.Cvr,
                Address = organization.Adress,
                Email = organization.Email,
                Phone = organization.Phone,
                Uuid = organization.Uuid,
                Name = organization.Name
            };
        }

        public OrganizationPermissionsResponseDTO ToPermissionsDTO(OrganizationPermissionsResult permissions)
        {
            return new()
            {
                Read = permissions.Read,
                Modify = permissions.Modify,
                Delete = permissions.Delete,
                ModifyCvr = permissions.ModifyCvr
            };
        }

        public UIModuleCustomizationResponseDTO ToUIModuleCustomizationResponseDTO(UIModuleCustomization uiModuleCustomization)
        {
            return new()
            {
                Module = uiModuleCustomization.Module,
                Nodes = ToCustomizedUINodeDTOs(uiModuleCustomization.Nodes).ToList()
            };
        }

        private IEnumerable<CustomizedUINodeResponseDTO> ToCustomizedUINodeDTOs(ICollection<CustomizedUINode> nodes)
        {
            return nodes.Select(ToCustomizedUINodeDTO);
        }

        private CustomizedUINodeResponseDTO ToCustomizedUINodeDTO(CustomizedUINode node)
        {
            return new()
            {
                Key = node.Key,
                Enabled = node.Enabled
            };
        }
    }
}