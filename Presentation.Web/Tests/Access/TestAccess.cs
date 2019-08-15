using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
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

        private static OrganizationContext SetupOrganizationContext(User user, AccessModifier organizationAccessModifier, IGenericRepository<ItSystemRole> mockSystemRoleRepository)
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
            return new OrganizationContext(mockUserRepository, mockOrganizationRepository, mockSystemRoleRepository, ExpectedOrganizationId);
        }

        private static OrganizationContext SetupOrganizationContext(User user, AccessModifier organizationAccessModifier)
        {
            var mockSystemRoleRepository = Substitute.For<IGenericRepository<ItSystemRole>>();
            return SetupOrganizationContext(user, organizationAccessModifier, mockSystemRoleRepository);
        }

        #endregion

        [TestCase(true, true, AccessModifier.Local, true)]
        [TestCase(true, false, AccessModifier.Local, true)]
        [TestCase(true, true, AccessModifier.Public, true)]
        [TestCase(false, true, AccessModifier.Local, true)]
        [TestCase(false, true, AccessModifier.Public, true)]
        [TestCase(false, false, AccessModifier.Local, false)]
        [TestCase(false, false, AccessModifier.Public, true)]
        public void AllowsRead_WhenUserInOrOutOfOrganization_Then_ReadAccessIsDeterminedCorrectly(
            bool isGlobalAdmin, bool inOrganization, AccessModifier organizationAccessModifier, bool expectedResult)
        {
            var user = SetupUser(isGlobalAdmin, inOrganization);
            var sut = SetupOrganizationContext(user, organizationAccessModifier);

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
            var system = new ItSystem
            {
                AccessModifier = systemAccessModifier
            };
            var user = SetupUser(isGlobalAdmin, inOrganization);
            var sut = SetupOrganizationContext(user, organizationAccessModifier);

            Assert.AreEqual(expectedResult, sut.AllowReads(user.Id, system));
        }

        [Test]
        public void AllowsUpdates_WhenUserInOrganization_Then_WriteAccessIsDeterminedCorrectly()
        {
            var user = SetupUser(false, true);
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
            var mockSystemRoleRepository = Substitute.For<IGenericRepository<ItSystemRole>>();
            mockSystemRoleRepository.AsQueryable().Returns(new List<ItSystemRole>() { itSystemRole }.AsQueryable());
            var sut = SetupOrganizationContext(user, AccessModifier.Local, mockSystemRoleRepository);
            Assert.AreEqual(true, sut.AllowUpdates(user.Id, itSystemUsage));
        }

        [Test]
        public void AllowsUpdates_WhenUserOutOfOrganization_Then_WriteAccessIsDeterminedCorrectly()
        {
            Assert.False(true);
        }
    }
}