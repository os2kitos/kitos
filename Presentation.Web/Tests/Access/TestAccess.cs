using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using NSubstitute;
using NUnit.Framework;
using Presentation.Web.Access;

namespace Presentation.Web.Tests.Access
{
    [TestFixture]
    public class TestAccess
    {
        [TestCase(true, true, AccessModifier.Local, true)]
        [TestCase(true, false, AccessModifier.Local, true)]
        [TestCase(true, true, AccessModifier.Public, true)]
        [TestCase(false, true, AccessModifier.Local, true)]
        [TestCase(false, true, AccessModifier.Public, true)]
        [TestCase(false, false, AccessModifier.Local, false)]
        [TestCase(false, false, AccessModifier.Public, true)]
        public void GivenOrganizationContext_WhenUserInOrOutOfOrganization_ThenReadAccessIsDeterminedCorrectly(
            bool isGlobalAdmin, bool inOrganization, AccessModifier accessModifier, bool expectedResult)
        {
            const int expectedOrganizationId = 1;
            const int differentOrganizationId = 2;
            var user = new User
            {
                DefaultOrganizationId = inOrganization ? expectedOrganizationId : differentOrganizationId,
                Id = 1,
                IsGlobalAdmin = isGlobalAdmin
            };
            var mockUserRepository = Substitute.For<IGenericRepository<User>>();
            mockUserRepository.GetByKey(user.Id).Returns(user);
            var mockOrganizationRepository = Substitute.For<IGenericRepository<Organization>>();
            var mockExpectedOrganization = Substitute.For<Organization>();
            mockExpectedOrganization.Id = expectedOrganizationId;
            var mockDifferentOrganization = Substitute.For<Organization>();
            mockDifferentOrganization.Id = differentOrganizationId;
            mockExpectedOrganization.AccessModifier = accessModifier;
            mockOrganizationRepository.GetByKey(expectedOrganizationId).Returns(mockExpectedOrganization);
            mockOrganizationRepository.GetByKey(differentOrganizationId).Returns(mockDifferentOrganization);
            var sut = new OrganizationContext(mockUserRepository, mockOrganizationRepository, expectedOrganizationId);
            Assert.AreEqual(sut.AllowReads(user.Id), expectedResult);
        }
    }
}