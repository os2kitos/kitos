using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Rights;
using Core.ApplicationServices.UIConfiguration.Handlers;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.SSO;
using Core.DomainServices.Repositories.SSO;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Handlers
{
    public class HandleUserBeingDeletedTests : WithAutoFixture
    {
        private readonly Mock<ISsoUserIdentityRepository> _ssoUserIdentityRepository;
        private readonly HandleUserBeingDeleted _sut;
        private readonly Mock<IUserRightsService> _userRightsServiceMock;

        public HandleUserBeingDeletedTests()
        {
            _ssoUserIdentityRepository = new Mock<ISsoUserIdentityRepository>();

            _userRightsServiceMock = new Mock<IUserRightsService>();
            _sut = new HandleUserBeingDeleted(_ssoUserIdentityRepository.Object, _userRightsServiceMock.Object);
        }

        [Fact]
        public void Handle_Deletes_All_User_Rights()
        {
            //Arrange
            var roleId = A<int>();
            var organizationRole = A<OrganizationRole>();
            var user = SetupUser(roleId, organizationRole);

            foreach (var organizationId in user.GetOrganizationIds())
            {
                _userRightsServiceMock.Setup(x => x.RemoveAllRights(user.Id, organizationId)).Returns(Maybe<OperationError>.None);
            }

            //act
            _sut.Handle(new EntityBeingDeletedEvent<User>(user));

            //assert
            _ssoUserIdentityRepository.Verify(x => x.DeleteIdentitiesForUser(user), Times.AtLeastOnce);
            Assert.Empty(user.SsoIdentities);

            foreach (var organizationId in user.GetOrganizationIds())
            {
                _userRightsServiceMock.Verify(x => x.RemoveAllRights(user.Id, organizationId), Times.Once);
            }
        }

        private User SetupUser(int roleId, OrganizationRole organizationRole)
        {
            var user = new User
            {
                Id = A<int>(),
                DataProcessingRegistrationRights = new List<DataProcessingRegistrationRight>(),
                OrganizationRights = new List<OrganizationRight>(),
                ItContractRights = new List<ItContractRight>(),
                ItSystemRights = new List<ItSystemRight>(),
                OrganizationUnitRights = new List<OrganizationUnitRight>(),
                SsoIdentities = new List<SsoUserIdentity>()
            };

            user.SsoIdentities.Add(new SsoUserIdentity());

            return user;
        }
    }
}
