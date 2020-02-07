using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class OrganizationalUserContextTest : WithAutoFixture
    {
        [Fact]
        public void IsActiveInOrganization_Returns_True_If_OrgId_Matches_ActiveOrgId()
        {
            //Arrange
            var activeOrganizationId = A<int>();
            var sut = new OrganizationalUserContext(Many<OrganizationRole>(), new User(), activeOrganizationId);

            //Act
            var result = sut.IsActiveInOrganization(activeOrganizationId);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void IsActiveInOrganization_Returns_False_If_OrgId_Differs_From_ActiveOrgId()
        {
            //Arrange
            var activeOrganizationId = A<int>();
            var otherOrgId = activeOrganizationId + 1;
            var sut = new OrganizationalUserContext(Many<OrganizationRole>(), new User(), activeOrganizationId);

            //Act
            var result = sut.IsActiveInOrganization(otherOrgId);

            //Assert
            Assert.False(result);
        }

        public interface IEntityWithContextAware : IEntity, IContextAware { }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsActiveInSameOrganizationAs_Returns_Result_Based_On_Active_Context_Query(bool contextResult)
        {
            //Arrange
            var activeOrganizationId = A<int>();
            var sut = new OrganizationalUserContext(Many<OrganizationRole>(), new User(), activeOrganizationId);
            var target = new Mock<IEntityWithContextAware>();
            target.Setup(x => x.IsInContext(activeOrganizationId)).Returns(contextResult);

            //Act
            var result = sut.IsActiveInSameOrganizationAs(target.Object);

            //Assert
            target.Verify();
            Assert.Equal(contextResult, result);
        }

        public interface IEntityWithOrganization : IEntity, IOwnedByOrganization { }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsActiveInSameOrganizationAs_Returns_Result_Based_On_Same_Organization_Som_OrganizationId_Property(bool sameOrg)
        {
            //Arrange
            var activeOrganizationId = A<int>();
            var sut = new OrganizationalUserContext(Many<OrganizationRole>(), new User(), activeOrganizationId);
            var target = new Mock<IEntityWithOrganization>();
            target.Setup(x => x.OrganizationId).Returns(sameOrg ? activeOrganizationId : A<int>());

            //Act
            var result = sut.IsActiveInSameOrganizationAs(target.Object);

            //Assert
            target.Verify();
            Assert.Equal(sameOrg, result);
        }

        [Theory]
        [MemberData(nameof(GetRoles))]
        public void HasRole_Returns_True_For_Supported_Roles_And_False_For_Unsupported(OrganizationRole unsupportedRole)
        {
            //Arrange
            var allRoles = Enum.GetValues(typeof(OrganizationRole)).Cast<OrganizationRole>().ToList();
            var supportedRoles = allRoles.Except(new[] { unsupportedRole }).ToList();
            var sut = new OrganizationalUserContext(supportedRoles, new User(), A<int>());

            //Act
            var results = allRoles.Select(role => new
            {
                Role = role,
                Result = sut.HasRole(role),
                ExpectedResult = role != unsupportedRole
            }).ToList();

            //Assert
            foreach (var result in results)
            {
                Assert.Equal(result.ExpectedResult, result.Result);
            }
        }

        [Fact]
        public void UserId_Returns_Provided_Users_Id()
        {
            var user = new User { Id = A<int>() };

            var sut = new OrganizationalUserContext(Many<OrganizationRole>(), user, A<int>());

            Assert.Equal(sut.UserId, user.Id);
        }

        [Fact]
        public void ActiveOrganizationId_Returns_Provided_OrganizationId()
        {
            var organizationId = A<int>();

            var sut = new OrganizationalUserContext(Many<OrganizationRole>(), new User(), organizationId);

            Assert.Equal(organizationId, sut.ActiveOrganizationId);
        }

        [Theory]
        [InlineData(OrganizationCategory.Municipality, OrganizationCategory.Municipality, true)]
        [InlineData(OrganizationCategory.Municipality, OrganizationCategory.Other, false)]
        [InlineData(OrganizationCategory.Other, OrganizationCategory.Other, true)]
        [InlineData(OrganizationCategory.Other, OrganizationCategory.Municipality, false)]
        public void IsActiveInOrganizationOfType_Returns(OrganizationCategory inputCategory, OrganizationCategory activeCategory, bool expectedResult)
        {
            //Arrange
            var user = new User
            {
                DefaultOrganization = new Organization
                {
                    Type = new OrganizationType
                    {
                        Category = activeCategory
                    }
                }
            };
            var sut = new OrganizationalUserContext(Many<OrganizationRole>(), user, A<int>());

            //Act
            var result = sut.IsActiveInOrganizationOfType(inputCategory);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasAssignedWriteAccess_Delegates_Question_To_Provided_Entity(bool hasAccess)
        {
            //Arrange
            var user = new User();
            var sut = new OrganizationalUserContext(Many<OrganizationRole>(), user, A<int>());
            var entity = Mock.Of<IEntity>(x => x.HasUserWriteAccess(user) == hasAccess);

            //Act
            var result = sut.HasAssignedWriteAccess(entity);

            //Assert
            Assert.Equal(hasAccess, result);
        }

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(1, 2, false)]
        [InlineData(2, 1, false)]
        public void HasOwnership_Returns_Based_On_OwnerId(int entityOwnerId, int userId, bool expectedResult)
        {
            //Arrange
            var user = new User() { Id = userId };
            var sut = new OrganizationalUserContext(Many<OrganizationRole>(), user, A<int>());
            var entity = Mock.Of<IEntity>(x => x.ObjectOwnerId == entityOwnerId);

            //Act
            var result = sut.HasOwnership(entity);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        #region helpers

        public static IEnumerable<object[]> GetNonSpecificVisibilityChangeTypeTestInputs()
        {
            yield return new object[] { new ItSystem(), true };
            yield return new object[] { new ItSystem(), false };

            yield return new object[] { new ItProject(), true };
            yield return new object[] { new ItProject(), false };

            yield return new object[] { new Report(), true };
            yield return new object[] { new Report(), false };
        }


        public static IEnumerable<object[]> GetRoles()
        {
            return Enum.GetValues(typeof(OrganizationRole)).Cast<object>().Select(x => new[] { x });
        }
        
        #endregion helpers
    }
}
