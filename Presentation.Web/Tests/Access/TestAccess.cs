using Core.DomainModel;
using Core.DomainModel.ItSystem;
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
        private const int ExpectedOrganizationId = 1;
        private const int DifferentOrganizationId = 2;

        #region Helpers

        private static ItSystem SetupSystem()
        {
            var system = new ItSystem();
            return system;
        }

        private static User SetupUser(bool isGlobalAdmin, bool inOrganization)
        {
            var user = new User
            {
                DefaultOrganizationId = inOrganization ? ExpectedOrganizationId : DifferentOrganizationId,
                Id = 1,
                IsGlobalAdmin = isGlobalAdmin
            };
            return user;
        }

        private static OrganizationContext SetupOrganizationContext(User user, ItSystem system, AccessModifier organizationAccessModifier)
        {
            var mockUserRepository = Substitute.For<IGenericRepository<User>>();
            mockUserRepository.GetByKey(user.Id).Returns(user);
            var mockOrganizationRepository = Substitute.For<IGenericRepository<Organization>>();
            var mockExpectedOrganization = Substitute.For<Organization>();
            mockExpectedOrganization.Id = ExpectedOrganizationId;
            var mockDifferentOrganization = Substitute.For<Organization>();
            mockDifferentOrganization.Id = DifferentOrganizationId;
            mockExpectedOrganization.AccessModifier = organizationAccessModifier;
            mockOrganizationRepository.GetByKey(ExpectedOrganizationId).Returns(mockExpectedOrganization);
            mockOrganizationRepository.GetByKey(DifferentOrganizationId).Returns(mockDifferentOrganization);
            return new OrganizationContext(mockUserRepository, mockOrganizationRepository, ExpectedOrganizationId);
        }

        #endregion

        [TestCase(true, true, AccessModifier.Local, true)]
        [TestCase(true, false, AccessModifier.Local, true)]
        [TestCase(true, true, AccessModifier.Public, true)]
        [TestCase(false, true, AccessModifier.Local, true)]
        [TestCase(false, true, AccessModifier.Public, true)]
        [TestCase(false, false, AccessModifier.Local, false)]
        [TestCase(false, false, AccessModifier.Public, true)]
        public void AllowsRead_WhenUserInOrOutOfOrganization_ThenReadAccessIsDeterminedCorrectly(
            bool isGlobalAdmin, bool inOrganization, AccessModifier organizationAccessModifier, bool expectedResult)
        {
            var system = SetupSystem();
            var user = SetupUser(isGlobalAdmin, inOrganization);
            var sut = SetupOrganizationContext(user, system, organizationAccessModifier);

            Assert.AreEqual(expectedResult, sut.AllowReads(user.Id));
        }

        [TestCase(true, true, AccessModifier.Local, AccessModifier.Local, true)]
        [TestCase(true, false, AccessModifier.Local, AccessModifier.Local, true)]
        [TestCase(false, true, AccessModifier.Local, AccessModifier.Local, true)]
        [TestCase(false, true, AccessModifier.Local, AccessModifier.Public, true)]
        [TestCase(false, false, AccessModifier.Local, AccessModifier.Local, false)]
        [TestCase(false, false, AccessModifier.Public, AccessModifier.Local, false)]
        [TestCase(false, false, AccessModifier.Public, AccessModifier.Public, true)]
        public void AllowsReadOnSystem_WhenUserInOrOutOfOrganization_Then_ReadAccessIsDeterminedCorrectly(
            bool isGlobalAdmin, bool inOrganization, AccessModifier organizationAccessModifier, AccessModifier systemAccessModifier, bool expectedResult)
        {
            var system = SetupSystem();
            system.AccessModifier = systemAccessModifier;
            var user = SetupUser(isGlobalAdmin, inOrganization);
            var sut = SetupOrganizationContext(user, system, organizationAccessModifier);

            Assert.AreEqual(expectedResult, sut.AllowReads(user.Id, system));
        }
    }
}