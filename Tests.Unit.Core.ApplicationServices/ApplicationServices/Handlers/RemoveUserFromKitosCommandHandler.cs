using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Users.Handlers;
using Core.DomainModel;
using Core.DomainModel.Commands;
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
    public class RemoveUserFromKitosCommandHandlerTest : WithAutoFixture
    {
        private readonly Mock<ISsoUserIdentityRepository> _ssoUserIdentityRepository;
        private readonly RemoveUserFromKitosCommandHandler _sut;
        private readonly Mock<ICommandBus> _commandBusMock;

        public RemoveUserFromKitosCommandHandlerTest()
        {
            _ssoUserIdentityRepository = new Mock<ISsoUserIdentityRepository>();

            _commandBusMock = new Mock<ICommandBus>();
            _sut = new RemoveUserFromKitosCommandHandler(_ssoUserIdentityRepository.Object, _commandBusMock.Object);
        }

        [Fact]
        public void Execute_Deletes_All_User_Rights()
        {
            //Arrange
            var user = SetupUser();

            foreach (var organizationId in user.GetOrganizationIds())
            {
                _commandBusMock.Setup(x =>
                    x.Execute<RemoveUserFromOrganizationCommand, Maybe<OperationError>>(
                        It.Is<RemoveUserFromOrganizationCommand>(c => c.User == user && c.OrganizationId == organizationId))).Returns(Maybe<OperationError>.None);
            }

            //act
            _sut.Execute(new RemoveUserFromKitosCommand(user));

            //assert
            _ssoUserIdentityRepository.Verify(x => x.DeleteIdentitiesForUser(user), Times.AtLeastOnce);
            Assert.Empty(user.SsoIdentities);

            foreach (var organizationId in user.GetOrganizationIds())
            {
                _commandBusMock.Verify(x => x.Execute<RemoveUserFromOrganizationCommand, Maybe<OperationError>>(It.Is<RemoveUserFromOrganizationCommand>(c => c.User == user && c.OrganizationId == organizationId)), Times.Once);
            }
        }

        private User SetupUser()
        {
            var user = new User
            {
                Id = A<int>(),
                DataProcessingRegistrationRights = new List<DataProcessingRegistrationRight>(),
                OrganizationRights = Many<int>().Distinct().Select(orgId => new OrganizationRight() { Organization = new Organization() { Id = orgId }, OrganizationId = orgId }).ToList(),
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
