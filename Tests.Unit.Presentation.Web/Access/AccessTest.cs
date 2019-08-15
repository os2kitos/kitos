using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using NSubstitute;
using Presentation.Web.Access;
using Xunit;

namespace Tests.Unit.Presentation.Web.Access
{
    public class AccessTest
    {
        private const int ExpectedOrganizationId = 1;
        private const int DifferentOrganizationId = 2;

        #region Helpers

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

        private static OrganizationContext SetupOrganizationContext(User user, AccessModifier organizationAccessModifier)
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

        [Theory]
        [InlineData(true, true, AccessModifier.Local, true)]
        [InlineData(true, false, AccessModifier.Local, true)]
        [InlineData(true, true, AccessModifier.Public, true)]
        [InlineData(false, true, AccessModifier.Local, true)]
        [InlineData(false, true, AccessModifier.Public, true)]
        [InlineData(false, false, AccessModifier.Local, false)]
        [InlineData(false, false, AccessModifier.Public, true)]
        public void AllowsRead_WhenUserInOrOutOfOrganization_Then_ReadAccessIsDeterminedCorrectly(
            bool isGlobalAdmin, bool inOrganization, AccessModifier organizationAccessModifier, bool expectedResult)
        {
            var user = SetupUser(isGlobalAdmin, inOrganization);
            var sut = SetupOrganizationContext(user, organizationAccessModifier);

            Assert.Equal(expectedResult, sut.AllowReads(user.Id));
        }

        [Theory]
        [InlineData(true, true, AccessModifier.Local, AccessModifier.Local, true)]
        [InlineData(true, false, AccessModifier.Local, AccessModifier.Local, true)]
        [InlineData(false, true, AccessModifier.Local, AccessModifier.Local, true)]
        [InlineData(false, true, AccessModifier.Local, AccessModifier.Public, true)]
        [InlineData(false, false, AccessModifier.Local, AccessModifier.Local, false)]
        [InlineData(false, false, AccessModifier.Public, AccessModifier.Local, false)]
        [InlineData(false, false, AccessModifier.Public, AccessModifier.Public, true)]
        public void AllowsReadOnSystem_WhenUserInOrOutOfOrganization_Then_ReadAccessIsDeterminedCorrectly(
            bool isGlobalAdmin, bool inOrganization, AccessModifier organizationAccessModifier, AccessModifier systemAccessModifier, bool expectedResult)
        {
            var system = new ItSystem
            {
                AccessModifier = systemAccessModifier
            };
            var user = SetupUser(isGlobalAdmin, inOrganization);
            var sut = SetupOrganizationContext(user, organizationAccessModifier);

            Assert.Equal(expectedResult, sut.AllowReads(user.Id, system));
        }
        
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public void AllowsUpdates_WhenUserInOrOutOfOrganization_Then_WriteAccessIsDeterminedCorrectly(bool inOrganization, bool expectedResult)
        {
            var user = SetupUser(false, inOrganization);
            var itSystemRole = new ItSystemRole
            {
                HasWriteAccess = true
            };
            var itSystemRight = new ItSystemRight
            {
                User = user,
                UserId = user.Id,
                Role = itSystemRole
            };
            var itSystemUsage = new ItSystemUsage();
            itSystemUsage.Rights.Add(itSystemRight);
            user.ItSystemRights.Add(itSystemRight);
            var sut = SetupOrganizationContext(user, AccessModifier.Local);
            Assert.Equal(expectedResult, sut.AllowUpdates(user.Id, itSystemUsage));
        }
    }
}