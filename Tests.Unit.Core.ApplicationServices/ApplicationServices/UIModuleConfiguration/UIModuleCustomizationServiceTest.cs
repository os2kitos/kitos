using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.UiCustomization;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.UIConfiguration;
using Core.DomainModel.Organization;
using Core.DomainModel.UIConfiguration;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
using Core.DomainServices.Repositories.UICustomization;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.UIModuleConfiguration
{
    public class UIModuleCustomizationServiceTest : WithAutoFixture
    {
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;
        private readonly Mock<IUIModuleCustomizationRepository> _repositoryMock;

        private readonly UIModuleCustomizationService _sut;

        public UIModuleCustomizationServiceTest()
        {
            _transactionManagerMock = new Mock<ITransactionManager>();
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _repositoryMock = new Mock<IUIModuleCustomizationRepository>();

            _sut = new UIModuleCustomizationService(
                _transactionManagerMock.Object,
                _userContextMock.Object,
                _organizationServiceMock.Object,
                _identityResolverMock.Object,
                _repositoryMock.Object);
        }

        [Theory]
        [InlineData("ItSystemUsages")]
        [InlineData("ItContracts")]
        [InlineData("DataProcessingRegistrations")]
        public void GET_Missing_Module_Returns_Empty_Configuration_Without_Modifying_Organization(string module)
        {
            var (organization, _) = SetupGetModuleCustomization();
            var originalCount = organization.UIModuleCustomizations.Count;
            _repositoryMock.Setup(x => x.GetByOrganizationAndModule(organization.Id, module))
                .Returns(Maybe<UIModuleCustomization>.None);

            var result = _sut.GetModuleCustomizationForOrganization(organization.Id, module);

            Assert.True(result.Ok);
            Assert.Equal(organization.Id, result.Value.OrganizationId);
            Assert.Empty(result.Value.Nodes);
            Assert.Equal(originalCount, organization.UIModuleCustomizations.Count);
            _repositoryMock.Verify(x => x.Update(It.IsAny<UIModuleCustomization>()), Times.Never);
        }

        [Fact]
        public void PUT_Returns_Updated_Module_Without_Reading_Again_And_Removes_Omitted_Nodes()
        {
            var (organization, module) = SetupGetModuleCustomization();
            var removedNode = Assert.Single(module.Nodes);
            ExpectTransactionBeginReturns();
            ExpectHasRoleReturns(organization.Id, OrganizationRole.LocalAdmin, true);

            var result = _sut.UpdateModuleAndGet(new UIModuleCustomizationParameters(
                organization.Id, module.Module, Array.Empty<CustomUINodeParameters>()));

            Assert.True(result.Ok);
            Assert.Same(module, result.Value);
            Assert.Empty(result.Value.Nodes);
            _repositoryMock.Verify(x => x.GetByOrganizationAndModule(organization.Id, module.Module), Times.Once);
            _repositoryMock.Verify(x => x.DeleteNodes(It.Is<IEnumerable<CustomizedUINode>>(nodes => nodes.Single() == removedNode)), Times.Once);
            _repositoryMock.Verify(x => x.Update(module), Times.Once);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PUT_Returns_NotFound_Without_Committing_When_Organization_Doesnt_Exist(bool resolvesUuid)
        {
            var parameters = PrepareTestUiModuleCustomizationParameters(A<int>(), "ItSystemUsages");
            var transaction = ExpectTransactionBeginReturns();
            var uuid = Guid.NewGuid();
            _identityResolverMock.Setup(x => x.ResolveUuid<Organization>(parameters.OrganizationId))
                .Returns(resolvesUuid ? uuid : Maybe<Guid>.None);
            ExpectOrganizationServiceGetReturns(Result<Organization, OperationError>.Failure(OperationFailure.NotFound), uuid);

            var result = _sut.UpdateModuleAndGet(parameters);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
            transaction.Verify(x => x.Dispose(), Times.Once);
            _repositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void PUT_Returns_Forbidden_Without_Committing_If_Not_LocalAdmin_In_Organization()
        {
            var (organization, module) = SetupGetModuleCustomization();
            var transaction = ExpectTransactionBeginReturns();
            ExpectHasRoleReturns(organization.Id, OrganizationRole.LocalAdmin, false);
            var parameters = PrepareTestUiModuleCustomizationParameters(organization.Id, module.Module);

            var result = _sut.UpdateModuleAndGet(parameters);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            _userContextMock.Verify(x => x.HasRole(organization.Id, OrganizationRole.LocalAdmin), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Never);
            transaction.Verify(x => x.Dispose(), Times.Once);
            _repositoryMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("1234")]
        [InlineData("Test-abc")]
        [InlineData("Test.123")]
        [InlineData("123.Test")]
        [InlineData("Test.?")]
        [InlineData("?.?")]
        [InlineData("_")]
        public void ModuleCustomization_Returns_BadInput_When_Key_Is_Incorrect(string key)
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
        public void ModuleCustomization_Returns_BadInput_When_Nodes_Contain_Duplicates()
        {
            //arrange
            var module = A<string>();
            var organization = new Organization{Id = 1};
            var testKey = "Test.Key";
            var nodes = PrepareTestNodes(2, testKey);

            //act
            var result = organization.ModifyModuleCustomization(module, nodes);

            //assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }
        
        [Fact]
        public void ModuleCustomization_References_Correct_Organization_After_Creating()
        {
            //arrange
            var module = A<string>();
            var organization = new Organization{Id = 1};
            var nodes = PrepareTestNodes();

            //act
            var result = organization.ModifyModuleCustomization(module, nodes);
            //assert
            Assert.Equal(organization.Id, result.Value.Organization.Id);
        }

        [Fact]
        public void ModuleCustomization_Mirrors_The_Nodes()
        {
            var module = A<string>();
            var organization = new Organization{Id = 1};
            var nodes = PrepareTestNodes();
            
            var result = organization.ModifyModuleCustomization(module, nodes);

            Assert.True(result.Ok);
            var resultValue = result.Value;
            Assert.Equal(nodes.Count, resultValue.Nodes.Count);

            Assert.Single(resultValue.Nodes);

            var resultNode = resultValue.Nodes.FirstOrDefault();
            var defaultNode = nodes.FirstOrDefault();

            Assert.NotNull(resultNode);
            Assert.NotNull(defaultNode);
            Assert.Equal(resultNode.Key, defaultNode.Key);
            Assert.Equal(resultNode.Enabled, defaultNode.Enabled);
            Assert.Equal(resultNode.Recommended, defaultNode.Recommended);
        }

        [Fact]
        public void GET_Returns_NotFound_When_Organization_Doesnt_Exist()
        {
            var organizationId = A<int>();
            var module = A<string>();
            var orgUuid = Guid.NewGuid();

            ExpectResolveUuidReturns(organizationId, orgUuid);
            ExpectOrganizationServiceGetReturns(Result<Organization, OperationError>.Failure(OperationFailure.NotFound), orgUuid);

            var result = _sut.GetModuleCustomizationForOrganization(organizationId, module);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GET_Returns_NotFound_If_UIModule_Doesnt_Exist()
        {
            var organizationId = A<int>();
            var module = A<string>();
            var orgUuid = Guid.NewGuid();

            ExpectResolveUuidReturns(organizationId, orgUuid);
            ExpectOrganizationServiceGetReturns(Result<Organization, OperationError>.Failure(OperationFailure.NotFound), orgUuid);

            var result = _sut.GetModuleCustomizationForOrganization(organizationId, module);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GET_Returns_List_Filtered_By_Module()
        {
            var (organization, moduleObject) = SetupGetModuleCustomization();

            var result = _sut.GetModuleCustomizationForOrganization(organization.Id, moduleObject.Module);
            
            Assert.True(result.Ok);
            Assert.NotNull(result.Value);
            Assert.Equal(moduleObject, result.Value);
        }

        [Fact]
        public void Can_Get_Module_Customization_By_Org_Uuid()
        {
            var (organization, moduleObject) = SetupGetModuleCustomization();
            ExpectResolveIdReturns(organization.Uuid, organization.Id);

            var result = _sut.GetModuleCustomizationByOrganizationUuid(organization.Uuid, moduleObject.Module);

            Assert.True(result.Ok);
            Assert.Equal(moduleObject, result.Value);
        }


        [Fact]
        public void Get_Module_Customization_By_Org_Uuid_Returns_Not_Found_If_No_Org_Id()
        {
            var (organization, moduleObject) = SetupGetModuleCustomization();
            ExpectResolveIdReturns(organization.Uuid, Maybe<int>.None);


            var result = _sut.GetModuleCustomizationByOrganizationUuid(organization.Uuid, moduleObject.Module);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        private (Organization, UIModuleCustomization) SetupGetModuleCustomization()
        {
            var organizationId = A<int>();
            var orgUuid = new Guid();
            var module1 = A<string>();
            var module2 = A<string>();
            var moduleObject1 = PrepareTestUiModuleCustomization(organizationId, module1);
            var moduleObject2 = PrepareTestUiModuleCustomization(organizationId, module2);
            var organization = new Organization
            {
                Id = organizationId,
                Uuid = orgUuid,
                UIModuleCustomizations = new List<UIModuleCustomization>()
                {
                    moduleObject1,
                    moduleObject2
                }
            };
            ExpectResolveUuidReturns(organizationId, orgUuid);
            ExpectOrganizationServiceGetReturns(Result<Organization, OperationError>.Success(organization), orgUuid);
            _repositoryMock.Setup(x => x.GetByOrganizationAndModule(organizationId, module1)).Returns(moduleObject1);
            return (organization, moduleObject1);
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
                nodes.Add(new CustomizedUINode{Key = key, Enabled = isEnabled, Recommended = isEnabled});
            }

            return nodes;
        }
        
        private UIModuleCustomizationParameters PrepareTestUiModuleCustomizationParameters(int orgId = 0, string module = "", int numberOfElements = 1, string key = "", bool isEnabled = false, bool isRecommended = false)
        {
            return new UIModuleCustomizationParameters(orgId, module, PrepareTestNodesParameters(numberOfElements, key, isEnabled, isRecommended));
        }

        private List<CustomUINodeParameters> PrepareTestNodesParameters(int numberOfElements = 1, string key = "", bool isEnabled = false, bool isRecommended = false)
        {
            var nodes = new List<CustomUINodeParameters>();
            for (var i = 0; i < numberOfElements; i++)
            {
                key = string.IsNullOrEmpty(key) ? GenerateKey() : key;
                nodes.Add(new CustomUINodeParameters(key, isEnabled, isRecommended));
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

        private void ExpectResolveIdReturns(Guid uuid, Maybe<int> dbId)
        {
            _identityResolverMock.Setup(x => x.ResolveDbId<Organization>(uuid)).Returns(dbId);
        }

        private Mock<IDatabaseTransaction> ExpectTransactionBeginReturns()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transaction.Object);
            return transaction;
        }

        private string GenerateKey()
        {
            return Regex.Replace(A<string>(), "[0-9-]", "a");
        }
    }
}
