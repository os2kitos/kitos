using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.UIConfiguration;
using Core.DomainModel.Organization;
using Core.DomainModel.UIConfiguration;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.UIModuleCustomisationTest
{
    public class UIModuleCustomisationTests : WithAutoFixture
    {
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;

        private readonly UIModuleCustomizationService _uiModuleCustomizationService;

        public UIModuleCustomisationTests()
        {
            _transactionManagerMock = new Mock<ITransactionManager>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationServiceMock = new Mock<IOrganizationService>();

            _uiModuleCustomizationService = new UIModuleCustomizationService(
                _transactionManagerMock.Object, 
                _authorizationContextMock.Object,
                _userContextMock.Object,
                _organizationServiceMock.Object);
        }

        [Theory]
        [InlineData("1234")]
        [InlineData("Test-abc")]
        [InlineData("Test.123")]
        [InlineData("123.Test")]
        [InlineData("Test.?")]
        [InlineData("?.?")]
        [InlineData("_")]
        public void ModuleCustomisation_Returns_BadInput_When_Key_Is_Incorrect(string key)
        {
            //arrange
            var module = A<string>();
            var organization = new Organization(){Id = 1};
            var nodes = PrepareTestNodes(1, key);

            //act
            var result = organization.ModifyModuleCustomization(module, nodes);

            //assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error);
        }
        
        [Fact]
        public void ModuleCustomisation_Returns_BadInput_When_Nodes_Contain_Duplicates()
        {
            //arrange
            var module = A<string>();
            var organization = new Organization(){Id = 1};
            var testKey = "Test.Key";
            var nodes = PrepareTestNodes(2, testKey);

            //act
            var result = organization.ModifyModuleCustomization(module, nodes);

            //assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error);
        }

        [Fact]
        public void ModuleCustomisation_Returns_BadInput_When_Nodes_Are_Empty()
        {
            //arrange
            var module = A<string>();
            var organization = new Organization{Id = 1};
            var nodes = PrepareTestNodes(0);

            //act
            var result = organization.ModifyModuleCustomization(module, nodes);

            //assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error);
        }

        [Fact]
        public void ModuleCustomisation_Is_Added_To_Organization_If_None_Exist()
        {
            var module = A<string>();
            var organization = new Organization{Id = 1};
            var nodes = PrepareTestNodes();
            
            Assert.Null(organization.UIModuleCustomizations);
            var result = organization.ModifyModuleCustomization(module, nodes);
            Assert.True(result.Ok);
            Assert.NotNull(organization.UIModuleCustomizations);
        }

        [Fact]
        public void ModuleCustomisation_Mirrors_The_Nodes()
        {
            var module = A<string>();
            var organization = new Organization{Id = 1};
            var nodes = PrepareTestNodes();
            
            var result = organization.ModifyModuleCustomization(module, nodes);

            Assert.True(result.Ok);
            Assert.Equal(nodes.Count, result.Value.Nodes.Count);
            Assert.NotNull(organization.UIModuleCustomizations);
        }

        [Fact]
        public void PUT_Returns_NotFound_When_Organization_Doesnt_Exist()
        {
            var organizationId = A<int>();
            var module = A<string>();
            var uiModule = PrepareTestUiModuleCustomization();

            var emptyOrganizationList = new List<Organization>();
            ExpectOrganizationServiceSearchReturns(emptyOrganizationList);

            var result = _uiModuleCustomizationService.Put(organizationId, module, uiModule);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void PUT_Returns_Forbidden_If_Not_LocalAdmin_In_Organization()
        {
            var organizationId = A<int>();
            var module = A<string>();
            var uiModule = PrepareTestUiModuleCustomization();
            var organizationRole = OrganizationRole.GlobalAdmin;

            var organizations = new List<Organization>
            {
                new()
                {
                    Id = organizationId
                }
            };
            ExpectOrganizationServiceSearchReturns(organizations);
            ExpectHasRoleReturns(organizationId, organizationRole, false);
            
            var result = _uiModuleCustomizationService.Put(organizationId, module, uiModule);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void GET_Returns_NotFound_When_Organization_Doesnt_Exist()
        {
            var organizationId = A<int>();
            var module = A<string>();

            var emptyOrganizationList = new List<Organization>();
            ExpectOrganizationServiceSearchReturns(emptyOrganizationList);

            var result = _uiModuleCustomizationService.GetModuleConfigurationForOrganization(organizationId, module);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void GET_Returns_Forbidden_If_Not_Authorized_To_Read()
        {
            var organizationId = A<int>();
            var module = A<string>();
            var organization = new Organization {Id = organizationId, UIModuleCustomizations = new List<UIModuleCustomization>()};

            var organizations = new List<Organization>
            {
                organization
            };
            ExpectOrganizationServiceSearchReturns(organizations);
            ExpectAllowReadsReturns(organization, false);
            
            var result = _uiModuleCustomizationService.GetModuleConfigurationForOrganization(organizationId, module);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void GET_Returns_NotFound_If_UIModule_Doesnt_Exist()
        {
            var organizationId = A<int>();
            var module = A<string>();
            var organization = new Organization { Id = organizationId, UIModuleCustomizations = new List<UIModuleCustomization>()};

            var organizations = new List<Organization>
            {
                organization
            };

            ExpectOrganizationServiceSearchReturns(organizations);
            ExpectAllowReadsReturns(organization, true);
            
            var result = _uiModuleCustomizationService.GetModuleConfigurationForOrganization(organizationId, module);
            
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void GET_Returns_List_Filtered_By_Module()
        {
            var organizationId = A<int>();
            var module1 = A<string>();
            var module2 = A<string>();
            var moduleObject1 = PrepareTestUiModuleCustomization(organizationId, module1);
            var moduleObject2 = PrepareTestUiModuleCustomization(organizationId, module2);
            var organization = new Organization
            {
                Id = organizationId, 
                UIModuleCustomizations = new List<UIModuleCustomization>()
                {
                    moduleObject1,
                    moduleObject2
                }
            };

            var organizations = new List<Organization>
            {
                organization
            };

            ExpectOrganizationServiceSearchReturns(organizations);
            ExpectAllowReadsReturns(organization, true);
            
            var result = _uiModuleCustomizationService.GetModuleConfigurationForOrganization(organizationId, module1);
            
            Assert.True(result.Ok);
            Assert.Single(result.Value);
            Assert.Contains(moduleObject1, result.Value);
        }

        private UIModuleCustomization PrepareTestUiModuleCustomization(int orgId = 0, string module = "", int numberOfElements = 1, string key = "", bool isEnabled = false)
        {
            return new UIModuleCustomization
            {
                OrganizationId = orgId,
                Module = module,
                Nodes = PrepareTestNodes(numberOfElements, key, isEnabled)
            };
        }

        private List<CustomizedUINode> PrepareTestNodes(int numberOfElements = 1, string key = "", bool isEnabled = false)
        {
            var nodes = new List<CustomizedUINode>(); 
            for (var i = 0; i < numberOfElements; i++)
            {
                key = string.IsNullOrEmpty(key) ? GenerateKey() : key;
                nodes.Add(new CustomizedUINode{Key = key, Enabled = isEnabled});
            }

            return nodes;
        }

        private void ExpectOrganizationServiceSearchReturns(List<Organization> orgList)
        {
            _organizationServiceMock.Setup(x => x.SearchAccessibleOrganizations()).Returns(orgList.AsQueryable);
        }

        private void ExpectHasRoleReturns(int organizationId, OrganizationRole role, bool value)
        {
            _userContextMock.Setup(x => x.HasRole(organizationId, role)).Returns(value);
        }

        private void ExpectAllowReadsReturns(Organization organization, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowReads(organization)).Returns(value);
        }

        private string GenerateKey()
        {
            return Regex.Replace(A<string>(), "[0-9-]", "a");
        }
    }
}
