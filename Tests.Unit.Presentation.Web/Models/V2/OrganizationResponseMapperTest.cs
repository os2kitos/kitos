using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.UIConfiguration;
using Moq;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Tests.Toolkit.Patterns;
using Tests.Unit.Presentation.Web.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class OrganizationResponseMapperTest: WithAutoFixture
    {
        private OrganizationResponseMapper _sut;
        private Mock<IOrganizationTypeMapper> _organizationTypeMapper;

        public OrganizationResponseMapperTest()
        {
            _organizationTypeMapper = new Mock<IOrganizationTypeMapper>();
            _sut = new OrganizationResponseMapper(_organizationTypeMapper.Object);
        }

        [Fact]
        public void Can_Map_To_UI_Root_Config_DTO()
        {
            var config = new Config()
            {
                ShowItContractModule = A<bool>(),
                ShowDataProcessing = A<bool>(),
                ShowItSystemModule = A<bool>()
            };

            var dto = _sut.ToUIRootConfigDTO(config);

            Assert.Equal(config.ShowItContractModule, dto.ShowItContractModule);
            Assert.Equal(config.ShowDataProcessing, dto.ShowDataProcessing);
            Assert.Equal(config.ShowItSystemModule, dto.ShowItSystemModule);
        }

        [Fact]
        public void Can_Map_To_UI_Customization_DTO()
        {
            var customization = new UIModuleCustomization()
            {
                Id = A<int>(),
                Module = A<string>(),
                Nodes = PrepareTestNodes(5, A<string>())
            };

            var result = _sut.ToUIModuleCustomizationResponseDTO(customization);

            Assert.Equal(customization.Module, result.Module);
            Assert.Equal(customization.Nodes.Count, result.Nodes.Count());
            foreach (var expectedNode in customization.Nodes)
            {
                var actual = result.Nodes.FirstOrDefault(nodeDto => nodeDto.Key == expectedNode.Key);

                Assert.NotNull(actual);
                Assert.Equal(expectedNode.Enabled, actual.Enabled);
            }
        }

        [Fact]
        public void Can_Map_To_Organization_DTO()
        {
            var organization = new Organization()
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
                Cvr = A<string>().AsCvr(),
                Type = new OrganizationType(){ Id = 1 }
            };
            var expectedOrganizationType =
                global::Presentation.Web.Models.API.V2.Types.Organization.OrganizationType.Municipality;
            _organizationTypeMapper.Setup(_ => _.MapOrganizationType(organization.Type))
                .Returns(expectedOrganizationType);

            var dto = _sut.ToOrganizationDTO(organization);

            Assert.Equal(organization.Uuid, dto.Uuid);
            Assert.Equal(organization.Name, dto.Name);
            Assert.Equal(organization.Cvr, dto.Cvr);
            Assert.Equal(expectedOrganizationType, dto.OrganizationType);
        }

        [Fact]
        public void Can_Map_To_Roles_Dto()
        {
            var contactPerson = new ContactPerson()
            {
                Email = A<string>(),
                Id = A<int>(),
                Name = A<string>(),
                PhoneNumber = A<string>()
            };
            var dataResponsible = new DataResponsible()
            {
                Email = A<string>(),
                Id = A<int>(),
                Name = A<string>(),
                Phone = A<string>()
            };
            var dataProtectionAdvisor = new DataProtectionAdvisor()
            {
                Email = A<string>(),
                Id = A<int>(),
                Name = A<string>(),
                Phone = A<string>()
            };
            var roles = new OrganizationMasterDataRoles()
            {
                ContactPerson = contactPerson,
                DataResponsible = dataResponsible,
                DataProtectionAdvisor = dataProtectionAdvisor
            };

            var result = _sut.ToRolesDTO(roles);

            var cp = result.ContactPerson;
            var dr = result.DataResponsible;
            var dpa = result.DataProtectionAdvisor;
            AssertContactPerson(contactPerson, cp);
            AssertDataResponsible(dataResponsible, dr);
            AssertDataProtectionAdvisor(dataProtectionAdvisor, dpa);
        }

        [Fact]
        public void Can_Map_To_Master_Data_Dto()
        {
            var organization = new Organization()
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
                Cvr = A<string>().AsCvr(),
                Email = A<string>(),
                Phone = A<string>(),
                Adress = A<string>()
            };
            
            var dto = _sut.ToMasterDataDTO(organization);

            Assert.Equal(organization.Uuid, dto.Uuid);
            Assert.Equal(organization.Name, dto.Name);
            Assert.Equal(organization.Cvr, dto.Cvr);
            Assert.Equal(organization.Email, dto.Email);
            Assert.Equal(organization.Adress, dto.Address);
        }

        [Fact]
        public void Can_Map_Permissions_Dto()
        {
            var permissions = new OrganizationPermissionsResult(A<bool>(), A<bool>(), A<bool>(), A<bool>());

            var dto = _sut.ToPermissionsDTO(permissions);

            Assert.Equal(permissions.Read, dto.Read);        
            Assert.Equal(permissions.Modify, dto.Modify);     
            Assert.Equal(permissions.Delete, dto.Delete);    
            Assert.Equal(permissions.ModifyCvr, dto.ModifyCvr);
        }

        private void AssertContactPerson(ContactPerson expected, ContactPersonResponseDTO actual)
        {
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.PhoneNumber, actual.PhoneNumber);
            Assert.Equal(expected.LastName, actual.LastName);
        }

        private void AssertDataResponsible(DataResponsible expected, DataResponsibleResponseDTO actual)
        {
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Cvr, actual.Cvr);
            Assert.Equal(expected.Phone, actual.Phone);
            Assert.Equal(expected.Adress, actual.Address);
        }

        private void AssertDataProtectionAdvisor(DataProtectionAdvisor expected, DataProtectionAdvisorResponseDTO actual)
        {
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Cvr, actual.Cvr);
            Assert.Equal(expected.Phone, actual.Phone);
            Assert.Equal(expected.Adress, actual.Address);
        }

        private List<CustomizedUINode> PrepareTestNodes(int numberOfElements = 1, string key = "", bool isEnabled = false)
        {
            var nodes = new List<CustomizedUINode>();
            for (var i = 0; i < numberOfElements; i++)
            {
                key = string.IsNullOrEmpty(key) ? GenerateKey() : key;
                nodes.Add(new CustomizedUINode { Key = key, Enabled = isEnabled });
            }

            return nodes;
        }

        private string GenerateKey()
        {
            return Regex.Replace(A<string>(), "[0-9-]", "a");
        }
    }
}
