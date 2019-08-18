using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;
using Moq;
using Presentation.Web.Access;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Access
{
    public class OrganizationalUserContextTest : WithAutoFixture
    {
        [Theory]
        [MemberData(nameof(GetModuleAccessTestInputs))]
        public void HasModuleLevelAccessTo_Returns_Correct_Result(IEntity entity, IReadOnlyList<Feature> supportedFeatures, bool expectedResult)
        {
            //Arrange
            var sut = new OrganizationalUserContext(supportedFeatures, Many<OrganizationRole>(), new User(), A<int>());

            //Act
            var result = sut.HasModuleLevelAccessTo(entity);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void IsActiveInOrganization_Returns_True_If_OrgId_Matches_ActiveOrgId()
        {
            //Arrange
            var activeOrganizationId = A<int>();
            var sut = new OrganizationalUserContext(Many<Feature>(), Many<OrganizationRole>(), new User(), activeOrganizationId);

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
            var sut = new OrganizationalUserContext(Many<Feature>(), Many<OrganizationRole>(), new User(), activeOrganizationId);

            //Act
            var result = sut.IsActiveInOrganization(otherOrgId);

            //Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsActiveInSameOrganizationAs_Returns_Result_Based_On_Active_Org_Id(bool contextResult)
        {
            //Arrange
            var activeOrganizationId = A<int>();
            var sut = new OrganizationalUserContext(Many<Feature>(), Many<OrganizationRole>(), new User(), activeOrganizationId);
            var target = new Mock<IContextAware>();
            target.Setup(x => x.IsInContext(activeOrganizationId)).Returns(contextResult);

            //Act
            var result = sut.IsActiveInSameOrganizationAs(target.Object);

            //Assert
            target.Verify();
            Assert.Equal(contextResult, result);
        }

        [Theory]
        [MemberData(nameof(GetRoles))]
        public void HasRole_Returns_True_For_Supported_Roles_And_False_For_Unsupported(OrganizationRole unsupportedRole)
        {
            //Arrange
            var allRoles = Enum.GetValues(typeof(OrganizationRole)).Cast<OrganizationRole>().ToList();
            var supportedRoles = allRoles.Except(new[] { unsupportedRole }).ToList();
            var sut = new OrganizationalUserContext(Many<Feature>(), supportedRoles, new User(), A<int>());

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
        public void User_Returns_Provided_User()
        {
            var user = new User();

            var sut = new OrganizationalUserContext(Many<Feature>(), Many<OrganizationRole>(), user, A<int>());

            Assert.Same(sut.User, user);
        }

        [Fact]
        public void ActiveOrganizationId_Returns_Provided_OrganizationId()
        {
            var organizationId = A<int>();

            var sut = new OrganizationalUserContext(Many<Feature>(), Many<OrganizationRole>(), new User(), organizationId);

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
            var sut = new OrganizationalUserContext(Many<Feature>(), Many<OrganizationRole>(), user, A<int>());

            //Act
            var result = sut.IsActiveInOrganizationOfType(inputCategory);

            //Assert
            Assert.Equal(expectedResult, result);
        }


        #region helpers

        public static IEnumerable<object[]> GetRoles()
        {
            return Enum.GetValues(typeof(OrganizationRole)).Cast<object>().Select(x => new[] { x });
        }

        public static IEnumerable<object[]> GetModuleAccessTestInputs()
        {
            //Systems
            yield return new object[] { Mock.Of<ItSystem>(), GetFeatureOptions(), true };
            yield return new object[] { Mock.Of<ItSystem>(), GetFeatureOptions(Feature.CanModifySystems), false };

            //Contracts
            yield return new object[] { Mock.Of<ItContract>(), GetFeatureOptions(), true };
            yield return new object[] { Mock.Of<ItContract>(), GetFeatureOptions(Feature.CanModifyContracts), false };

            //Organizations
            yield return new object[] { Mock.Of<Organization>(), GetFeatureOptions(), true };
            yield return new object[] { Mock.Of<Organization>(), GetFeatureOptions(Feature.CanModifyOrganizations), false };

            //Projects
            yield return new object[] { Mock.Of<ItProject>(), GetFeatureOptions(), true };
            yield return new object[] { Mock.Of<ItProject>(), GetFeatureOptions(Feature.CanModifyProjects), false };

            //Users
            yield return new object[] { Mock.Of<User>(), GetFeatureOptions(), true };
            yield return new object[] { Mock.Of<User>(), GetFeatureOptions(Feature.CanModifyUsers), false };

            //Users
            yield return new object[] { Mock.Of<Report>(), GetFeatureOptions(), true };
            yield return new object[] { Mock.Of<Report>(), GetFeatureOptions(Feature.CanModifyReports), false };
        }

        private static IReadOnlyList<Feature> GetFeatureOptions(params Feature[] unsupportedFeatures)
        {
            return
                Enum
                    .GetValues(typeof(Feature))
                    .Cast<Feature>()
                    .Except(unsupportedFeatures)
                    .ToList();
        }
        #endregion helpers
    }
}
