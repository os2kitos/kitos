using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;
using Core.DomainServices.Organizations;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;
using Xunit.Abstractions;

namespace Tests.Unit.Core.ApplicationServices.Organizations
{
    public class StsOrganizationSynchronizationServiceTest : WithAutoFixture
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IStsOrganizationUnitService> _stsOrganizationUnitService;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly StsOrganizationSynchronizationService _sut;

        public StsOrganizationSynchronizationServiceTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _stsOrganizationUnitService = new Mock<IStsOrganizationUnitService>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _sut = new StsOrganizationSynchronizationService(_authorizationContextMock.Object, _stsOrganizationUnitService.Object, _organizationServiceMock.Object, Mock.Of<ILogger>(), Mock.Of<IStsOrganizationService>(), null, null);
        }

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            fixture.Inject<IEnumerable<ExternalOrganizationUnit>>(Array.Empty<ExternalOrganizationUnit>()); //prevent endless loop when creating StsOrganizationUnit
        }

        [Fact]
        public void GetStsOrganizationalHierarchy_Without_Levels_Limit_Returns_Full_Tree()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            var children = Many<ExternalOrganizationUnit>();
            var root = new ExternalOrganizationUnit(A<Guid>(), A<string>(), A<Dictionary<string,string>>(), children);
            SetupGetOrganizationReturns(organizationId, organization);
            SetupResolveOrganizationTreeReturns(organization, root);
            SetupHasPermissionReturns(organization, true);

            //Act
            var result = _sut.GetStsOrganizationalHierarchy(organizationId, Maybe<int>.None);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(root, result.Value); //If root is unmodified, then no filtering happened
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GetStsOrganizationalHierarchy_With_Level_Limit_Returns_Filtered_Tree(int levels)
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            var children = Many<ExternalOrganizationUnit>();
            var root = new ExternalOrganizationUnit(A<Guid>(), A<string>(), A<Dictionary<string,string>>(), children);
            SetupGetOrganizationReturns(organizationId, organization);
            SetupResolveOrganizationTreeReturns(organization, root);
            SetupHasPermissionReturns(organization, true);

            //Act
            var result = _sut.GetStsOrganizationalHierarchy(organizationId, levels);

            //Assert
            Assert.True(result.Ok);
            Assert.NotSame(root, result.Value);
            Assert.Equal(levels, CountMaxLevels(result.Value));
            Assert.True(Compare(root, result.Value, levels - 1));
        }

        [Fact]
        public void GetStsOrganizationalHierarchy_With_Invalid_Limit_Fails()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            var children = Many<ExternalOrganizationUnit>();
            var root = new ExternalOrganizationUnit(A<Guid>(), A<string>(), A<Dictionary<string,string>>(), children);
            SetupGetOrganizationReturns(organizationId, organization);
            SetupResolveOrganizationTreeReturns(organization, root);
            SetupHasPermissionReturns(organization, true);

            //Act
            var result = _sut.GetStsOrganizationalHierarchy(organizationId, 0);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void GetStsOrganizationalHierarchy_Fails_If_Organization_Cannot_Be_Loaded()
        {
            //Arrange
            var organizationId = A<Guid>();
            var operationError = A<OperationError>();
            SetupGetOrganizationReturns(organizationId, operationError);

            //Act
            var result = _sut.GetStsOrganizationalHierarchy(organizationId, Maybe<int>.None);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(operationError, result.Error);
        }

        [Fact]
        public void GetStsOrganizationalHierarchy_Fails_If_User_Lacks_Permission()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            var children = Many<ExternalOrganizationUnit>();
            var root = new ExternalOrganizationUnit(A<Guid>(), A<string>(), A<Dictionary<string,string>>(), children);
            SetupGetOrganizationReturns(organizationId, organization);
            SetupResolveOrganizationTreeReturns(organization, root);
            SetupHasPermissionReturns(organization, false);

            //Act
            var result = _sut.GetStsOrganizationalHierarchy(organizationId, Maybe<int>.None);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetStsOrganizationalHierarchy_Fails_If_LoadHierarchy_Fails()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            var operationError = A<DetailedOperationError<ResolveOrganizationTreeError>>();
            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            SetupResolveOrganizationTreeReturns(organization, operationError);

            //Act
            var result = _sut.GetStsOrganizationalHierarchy(organizationId, Maybe<int>.None);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
        }

        private void SetupResolveOrganizationTreeReturns(Organization organization, Result<ExternalOrganizationUnit, DetailedOperationError<ResolveOrganizationTreeError>> root)
        {
            _stsOrganizationUnitService.Setup(x => x.ResolveOrganizationTree(organization)).Returns(root);
        }

        private void SetupGetOrganizationReturns(Guid organizationId, Result<Organization, OperationError> organization)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(organizationId, null)).Returns(organization);
        }

        private void SetupHasPermissionReturns(Organization organization, bool value)
        {
            _authorizationContextMock.Setup(x =>
                    x.HasPermission(It.Is<ImportHierarchyFromStsOrganizationPermission>(p => p.Organization == organization)))
                .Returns(value);
        }

        private static int CountMaxLevels(ExternalOrganizationUnit unit)
        {
            const int currentLevelContribution = 1;
            return unit
                .Children
                .Select(CountMaxLevels)
                .OrderByDescending(max => max)
                .FirstOrDefault() + currentLevelContribution;
        }

        private bool Compare(ExternalOrganizationUnit original, ExternalOrganizationUnit filtered, int levelsLeftToCompare)
        {
            Assert.Equal(original.Uuid, filtered.Uuid);
            Assert.Equal(original.Name, filtered.Name);
            Assert.Equal(original.MetaData, filtered.MetaData);
            if (levelsLeftToCompare > 0)
            {
                levelsLeftToCompare--;
                var originalChildren = original.Children.ToList();
                var filteredChildren = filtered.Children.ToList();
                Assert.Equal(originalChildren.Capacity, filteredChildren.Count);
                for (var i = 0; i < originalChildren.Count; i++)
                {
                    var originalChild = originalChildren[i];
                    var filteredChild = filteredChildren[i];
                    if (!Compare(originalChild, filteredChild, levelsLeftToCompare))
                    {
                        _testOutputHelper.WriteLine("Failed during validation of {original} vs {filtered} with {levels} levels left", originalChild, filteredChild, levelsLeftToCompare);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
