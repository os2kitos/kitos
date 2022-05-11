using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.UIConfiguration;
using Core.DomainModel.Organization;
using Core.DomainModel.UIConfiguration;
using Core.DomainServices.UIConfiguration;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.UIModuleCustomisationTest
{
    public class UIModuleCustomisationTests : WithAutoFixture
    {
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IUIModuleCustomizationRepository> _repositoryMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IUIModuleCustomizationService> _uiModuleCustomizationServiceMock;

        public UIModuleCustomisationTests()
        {
            _transactionManagerMock = new Mock<ITransactionManager>();
            _repositoryMock = new Mock<IUIModuleCustomizationRepository>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _uiModuleCustomizationServiceMock = new Mock<IUIModuleCustomizationService>();
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
            var organization = new Organization(){Id = 1};
            var nodes = PrepareTestNodes();

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

        /*[Fact]
        public void Can_Put()
        {
            //Arrange
            var orgId = A<int>();
            var module = A<string>();
            var uiModuleCustomization = PrepareTestUiModuleCustomizationDto();

            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transaction.Object);
            _uiModuleCustomizationServiceMock.Setup(x => x.Put(orgId, module, uiModuleCustomization));
            //Act

            //Assert
        }*/

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

        private string GenerateKey()
        {
            return Regex.Replace(A<string>(), "[0-9-]", "a");
        }
    }
}
