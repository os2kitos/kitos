using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.UiCustomization;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.UIConfiguration;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.UIConfiguration;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
using Core.DomainServices.Repositories.UICustomization;
using Infrastructure.DataAccess;
using Infrastructure.DataAccess.Services;
using Infrastructure.Services.DataAccess;
using Microsoft.OData.Edm;
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
        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;
        private readonly Mock<IUIModuleCustomizationRepository> _organizationRepositoryMock;

        private readonly UIModuleCustomizationService _uiModuleCustomizationService;

        public UIModuleCustomisationTests()
        {
            _transactionManagerMock = new Mock<ITransactionManager>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _organizationRepositoryMock = new Mock<IUIModuleCustomizationRepository>();

            _uiModuleCustomizationService = new UIModuleCustomizationService(
                _transactionManagerMock.Object,
                _userContextMock.Object,
                _organizationServiceMock.Object,
                _identityResolverMock.Object,
                _organizationRepositoryMock.Object);
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
            var organization = new Organization{Id = 1};
            var nodes = PrepareTestNodes(1, key);

            //act
            var result = organization.ModifyModuleCustomization(module, nodes);

            //assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
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
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
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
            var uiModule = PrepareTestUiModuleCustomizationParameters();
            var orgUuid = Guid.NewGuid();

            ExpectTransactionBeginReturns();
            ExpectResolveUuidReturns(uiModule.OrganizationId, orgUuid);
            ExpectOrganizationServiceGetReturns(Result<Organization, OperationError>.Failure(OperationFailure.NotFound), orgUuid);

            Maybe<Organization> test = new Organization();

            var result = _uiModuleCustomizationService.UpdateModule(uiModule);

            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.NotFound, result.Value.FailureType);
        }

        [Fact]
        public void PUT_Returns_Forbidden_If_Not_LocalAdmin_In_Organization()
        {
            var uiModule = PrepareTestUiModuleCustomizationParameters();
            var organizationRole = OrganizationRole.GlobalAdmin;
            var orgUuid = Guid.NewGuid();

            ExpectTransactionBeginReturns();
            ExpectResolveUuidReturns(uiModule.OrganizationId, orgUuid);
            ExpectOrganizationServiceGetReturns(Result<Organization, OperationError>.Success(new Organization()), orgUuid);
            ExpectHasRoleReturns(uiModule.OrganizationId, organizationRole, false);
            
            var result = _uiModuleCustomizationService.UpdateModule(uiModule);

            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }

        [Fact]
        public void GET_Returns_NotFound_When_Organization_Doesnt_Exist()
        {
            var organizationId = A<int>();
            var module = A<string>();
            var orgUuid = Guid.NewGuid();

            var emptyOrganizationList = new List<Organization>();
            ExpectResolveUuidReturns(organizationId, orgUuid);
            ExpectOrganizationServiceGetReturns(Result<Organization, OperationError>.Failure(OperationFailure.NotFound), orgUuid);

            var result = _uiModuleCustomizationService.GetModuleConfigurationForOrganization(organizationId, module);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GET_Returns_NotFound_If_UIModule_Doesnt_Exist()
        {
            var organizationId = A<int>();
            var module = A<string>();
            var orgUuid = Guid.NewGuid();

            ExpectOrganizationServiceGetReturns(Result<Organization, OperationError>.Failure(OperationFailure.NotFound), orgUuid);
            ExpectResolveUuidReturns(organizationId, orgUuid);

            var result = _uiModuleCustomizationService.GetModuleConfigurationForOrganization(organizationId, module);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GET_Returns_Forbidden_If_Not_Authorized_To_Read()
        {
            var organizationId = A<int>();
            var module = A<string>();
            var organization = new Organization {Id = organizationId, UIModuleCustomizations = new List<UIModuleCustomization>()};
            var orgUuid = Guid.NewGuid();

            var organizations = new List<Organization>
            {
                organization
            };

            ExpectResolveUuidReturns(organizationId, orgUuid);
            ExpectOrganizationServiceGetReturns(Result<Organization, OperationError>.Success(new Organization()), orgUuid);

            var result = _uiModuleCustomizationService.GetModuleConfigurationForOrganization(organizationId, module);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GET_Returns_List_Filtered_By_Module()
        {
            var organizationId = A<int>();
            var module1 = A<string>();
            var module2 = A<string>();
            var moduleObject1 = PrepareTestUiModuleCustomization(organizationId, module1);
            var moduleObject2 = PrepareTestUiModuleCustomization(organizationId, module2);
            var orgUuid = Guid.NewGuid();
            var organization = new Organization
            {
                Id = organizationId, 
                UIModuleCustomizations = new List<UIModuleCustomization>()
                {
                    moduleObject1,
                    moduleObject2
                }
            };

            ExpectResolveUuidReturns(organizationId, orgUuid);
            ExpectOrganizationServiceGetReturns(Result<Organization, OperationError>.Success(organization), orgUuid);

            var result = _uiModuleCustomizationService.GetModuleConfigurationForOrganization(organizationId, module1);
            
            Assert.True(result.Ok);
            Assert.NotNull(result.Value);
            Assert.Equal(moduleObject1, result.Value);
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
        
        private UIModuleCustomizationParameters PrepareTestUiModuleCustomizationParameters(int orgId = 0, string module = "", int numberOfElements = 1, string key = "", bool isEnabled = false)
        {
            return new UIModuleCustomizationParameters(orgId, module, PrepareTestNodesParameters(numberOfElements, key, isEnabled));
        }

        private List<CustomUINodeParameters> PrepareTestNodesParameters(int numberOfElements = 1, string key = "", bool isEnabled = false)
        {
            var nodes = new List<CustomUINodeParameters>();
            for (var i = 0; i < numberOfElements; i++)
            {
                key = string.IsNullOrEmpty(key) ? GenerateKey() : key;
                nodes.Add(new CustomUINodeParameters(key, isEnabled));
            }

            return nodes;
        }

        private void ExpectOrganizationServiceGetReturns(Result<Organization, OperationError> value, Guid uuid)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(uuid, OrganizationDataReadAccessLevel.All)).Returns(value);
        }

        private void ExpectHasRoleReturns(int organizationId, OrganizationRole role, bool value)
        {
            _userContextMock.Setup(x => x.HasRole(organizationId, role)).Returns(value);
        }
        
        private void ExpectResolveUuidReturns(int organizationId, Guid uuid)
        {
            _identityResolverMock.Setup(x => x.ResolveUuid<Organization>(organizationId)).Returns(uuid);
        }
        
        private void ExpectTransactionBeginReturns()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transaction.Object);
        }

        private string GenerateKey()
        {
            return Regex.Replace(A<string>(), "[0-9-]", "a");
        }
    }
}
