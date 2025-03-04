using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class OrganizationalUserContextTest : WithAutoFixture
    {
        private int _userId;
        private int _otherOrgTypeOrganizationId;
        private int _municipalityOrganizationId;
        private OrganizationalUserContext _sut;
        private int _unknownOrganizationId;
        private IReadOnlyDictionary<int, IEnumerable<OrganizationRole>> _rolesPerOrganizationId;
        private IReadOnlyDictionary<int, OrganizationCategory> _categoryPerOrganizationId;

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            SetupSut();
        }

        private void SetupSut(
            IReadOnlyDictionary<int, IEnumerable<OrganizationRole>> roleMap = null,
            IReadOnlyDictionary<int, OrganizationCategory> categoryMap = null,
            bool isStakeHolder = true, bool isSystemIntegrator = true)
        {
            _userId = A<int>();
            _municipalityOrganizationId = A<int>();
            _otherOrgTypeOrganizationId = _municipalityOrganizationId + 1;
            _unknownOrganizationId = _otherOrgTypeOrganizationId + 1;
            _rolesPerOrganizationId = roleMap ?? new Dictionary<int, IEnumerable<OrganizationRole>>
            {
                {_municipalityOrganizationId, A<IEnumerable<OrganizationRole>>()},
                {_otherOrgTypeOrganizationId, A<IEnumerable<OrganizationRole>>()}
            }.AsReadOnly();

            _categoryPerOrganizationId = categoryMap ?? new Dictionary<int, OrganizationCategory>
            {
                {_municipalityOrganizationId, OrganizationCategory.Municipality},
                {_otherOrgTypeOrganizationId, OrganizationCategory.Other}
            }.AsReadOnly();

            _sut = new OrganizationalUserContext(
                _userId,
                _rolesPerOrganizationId,
                _categoryPerOrganizationId,
                isStakeHolder,
                isSystemIntegrator
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsGlobalAdmin_Returns_Has_Global_Admin_In_Any_Organization(bool expectedResult)
        {
            //Arrange
            SetupSut(new Dictionary<int, IEnumerable<OrganizationRole>>
            {
                {_municipalityOrganizationId,new [] {OrganizationRole.ContractModuleAdmin}},
                {_otherOrgTypeOrganizationId,new [] {expectedResult ? OrganizationRole.GlobalAdmin : OrganizationRole.SystemModuleAdmin}}
            });

            //Act
            var result = _sut.IsGlobalAdmin();

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(OrganizationCategory.Other)]
        [InlineData(OrganizationCategory.Municipality)]
        public void HasRoleIn_Returns_True_If_OrgId_Matches_Id_User_Is_Member_Of(OrganizationCategory category)
        {
            //Arrange
            var organizationId = GetOrganizationId(category);

            //Act
            var result = _sut.HasRoleIn(organizationId);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void HasRoleIn_Returns_False_If_OrgId_Differs_Org_User_Has_Role_In()
        {
            //Act
            var result = _sut.HasRoleIn(_unknownOrganizationId);

            //Assert
            Assert.False(result);
        }

        public interface IEntityWithOrganizationMembership : IEntity, IIsPartOfOrganization { }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasRoleInSameOrganizationAs_Returns_Result(bool expectedResult)
        {
            //Arrange
            var organizationId = expectedResult ? GetOrganizationId(OrganizationCategory.Municipality) : _unknownOrganizationId;
            var target = new Mock<IEntityWithOrganizationMembership>();
            target.Setup(x => x.GetOrganizationIds()).Returns(new[] { organizationId });

            //Act
            var result = _sut.HasRoleInSameOrganizationAs(target.Object);

            //Assert
            target.Verify();
            Assert.Equal(expectedResult, result);
        }

        public interface IEntityWithOrganization : IEntity, IOwnedByOrganization { }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasRoleInSameOrganizationAs_Returns_Result_Based_On_Same_Organization_Som_OrganizationId_Property(bool sameOrg)
        {
            //Arrange
            var organizationId = sameOrg ? GetOrganizationId(OrganizationCategory.Municipality) : _unknownOrganizationId;
            var target = new Mock<IEntityWithOrganization>();
            target.Setup(x => x.OrganizationId).Returns(organizationId);

            //Act
            var result = _sut.HasRoleInSameOrganizationAs(target.Object);

            //Assert
            target.Verify();
            Assert.Equal(sameOrg, result);
        }

        [Theory]
        [InlineData(OrganizationCategory.Municipality)]
        [InlineData(OrganizationCategory.Other)]
        public void HasRole_Returns_True_For_Supported_Roles_And_False_For_Unsupported(OrganizationCategory category)
        {
            //Arrange
            var organizationId = GetOrganizationId(category);
            var allRoles = Enum.GetValues(typeof(OrganizationRole)).Cast<OrganizationRole>().ToList();
            var supportedRoles = GetSupportedRoles(category);

            //Act
            var results = allRoles.Select(role => new
            {
                Role = role,
                Result = _sut.HasRole(organizationId, role),
                ExpectedResult = supportedRoles.Contains(role)
            }).ToList();

            //Assert
            foreach (var result in results)
            {
                Assert.Equal(result.ExpectedResult, result.Result);
            }
        }

        [Theory]
        [InlineData(OrganizationCategory.Municipality)]
        [InlineData(OrganizationCategory.Other)]
        public void HasRoleInOrganizationOfType_Returns_True(OrganizationCategory category)
        {
            Assert.True(_sut.HasRoleInOrganizationOfType(category));
        }

        [Theory]
        [InlineData(OrganizationCategory.Municipality)]
        [InlineData(OrganizationCategory.Other)]
        public void HasRoleInOrganizationOfType_Returns_False(OrganizationCategory unsupportedCategory)
        {
            //Arrange
            OrganizationCategory supportedCategory;
            switch (unsupportedCategory)
            {
                case OrganizationCategory.Other:
                    supportedCategory = OrganizationCategory.Municipality;
                    break;
                case OrganizationCategory.Municipality:
                    supportedCategory = OrganizationCategory.Other;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(unsupportedCategory), unsupportedCategory, null);
            }

            SetupSut(categoryMap: new Dictionary<int, OrganizationCategory> { { _municipalityOrganizationId, supportedCategory }, { _otherOrgTypeOrganizationId, supportedCategory } });

            //Act
            var result = _sut.HasRoleInOrganizationOfType(unsupportedCategory);

            //Assert
            Assert.False(result);
        }

        private IEnumerable<OrganizationRole> GetSupportedRoles(OrganizationCategory category)
        {
            return _rolesPerOrganizationId[GetOrganizationId(category)];
        }

        [Fact]
        public void UserId_Returns_Provided_Users_Id()
        {
            Assert.Equal(_userId, _sut.UserId);
        }

        [Fact]
        public void OrganizationIds_Returns_OrganizationIds_With_Roles()
        {
            Assert.Equal(_rolesPerOrganizationId.Keys, _sut.OrganizationIds);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasStakeHolderAccess_Returns(bool expected)
        {
            //Arrange
            SetupSut(isStakeHolder:expected);

            //Act
            var result = _sut.HasStakeHolderAccess();

            //Assert
            Assert.Equal(expected,result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Is_System_Integrator_Returns(bool expected)
        {
            SetupSut(isSystemIntegrator:expected);

            var result = _sut.IsSystemIntegrator();

            Assert.Equal(expected,result);
        }

        #region helpers

        public static IEnumerable<object[]> GetNonSpecificVisibilityChangeTypeTestInputs()
        {
            yield return new object[] { new ItSystem(), true };
            yield return new object[] { new ItSystem(), false };
        }

        #endregion helpers

        private int GetOrganizationId(OrganizationCategory category)
        {
            switch (category)
            {
                case OrganizationCategory.Other:
                    return _otherOrgTypeOrganizationId;
                case OrganizationCategory.Municipality:
                    return _municipalityOrganizationId;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }
    }
}
