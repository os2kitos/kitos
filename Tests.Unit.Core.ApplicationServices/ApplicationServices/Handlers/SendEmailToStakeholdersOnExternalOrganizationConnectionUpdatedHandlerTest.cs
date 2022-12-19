using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Core.ApplicationServices.Organizations.Handlers;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Infrastructure.Services.Configuration;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Handlers
{
    public class SendEmailToStakeholdersOnExternalOrganizationConnectionUpdatedHandlerTest : WithAutoFixture
    {
        private readonly SendEmailToStakeholdersOnExternalOrganizationConnectionUpdatedHandler _sut;
        private readonly Mock<IMailClient> _mailClientMock;

        public SendEmailToStakeholdersOnExternalOrganizationConnectionUpdatedHandlerTest()
        {
            _mailClientMock = new Mock<IMailClient>();
            _sut = new SendEmailToStakeholdersOnExternalOrganizationConnectionUpdatedHandler(_mailClientMock.Object, Mock.Of<ILogger>(), new KitosUrl(new Uri("https://kitos-test.dk")));
        }

        [Fact]
        public void Handle_Ignores_Event_If_UserInitiated()
        {
            //Arrange
            var domainEvent = CreateEvent(new Organization(), OrganizationUnitOrigin.STS_Organisation, ExternalOrganizationChangeLogResponsible.User, Many<ExternalConnectionAddNewLogEntryInput>());

            //Act
            _sut.Handle(domainEvent);

            //Assert
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never());
        }

        [Fact]
        public void Handle_Ignores_Event_If_Origin_Is_Not_Sts_Org()
        {
            //Arrange
            var domainEvent = CreateEvent(new Organization(), OrganizationUnitOrigin.Kitos, ExternalOrganizationChangeLogResponsible.Background, Many<ExternalConnectionAddNewLogEntryInput>());

            //Act
            _sut.Handle(domainEvent);

            //Assert
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never());
        }

        [Fact]
        public void Handle_Ignores_Event_If_No_Changes()
        {
            //Arrange
            var domainEvent = CreateEvent(new Organization(), OrganizationUnitOrigin.STS_Organisation, ExternalOrganizationChangeLogResponsible.Background, Array.Empty<ExternalConnectionAddNewLogEntryInput>());

            //Act
            _sut.Handle(domainEvent);

            //Assert
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never());
        }

        [Fact]
        public void Handle_Sends_Email_To_LocalAdmins_If_Changes_Exist_And_Background_Job_for_Sts_Org()
        {
            //Arrange
            var expectedRightMatch1 = CreateRight(OrganizationRole.LocalAdmin);
            var expectedRightMatch2 = CreateRight(OrganizationRole.LocalAdmin);
            var organization = new Organization()
            {
                Rights = new List<OrganizationRight>()
                {
                    expectedRightMatch1,
                    CreateRight(OrganizationRole.User),
                    expectedRightMatch2

                }
            };
            var domainEvent = CreateEvent(organization, OrganizationUnitOrigin.STS_Organisation, ExternalOrganizationChangeLogResponsible.Background, Many<ExternalConnectionAddNewLogEntryInput>());

            //Act
            _sut.Handle(domainEvent);

            //Assert
            var expectedEmails = new[] { expectedRightMatch1.User.Email, expectedRightMatch2.User.Email };
            _mailClientMock.Verify(x => x.Send(It.Is<MailMessage>(message => message.To.Select(x => x.Address).SequenceEqual(expectedEmails))), Times.Once());
        }

        private OrganizationRight CreateRight(OrganizationRole organizationRole)
        {
            return new()
            {
                Role = organizationRole,
                User = new User()
                {
                    Email = $"{A<Guid>():N}@test.dk"
                }
            };
        }

        private ExternalOrganizationConnectionUpdated CreateEvent(Organization organization, OrganizationUnitOrigin organizationUnitOrigin, ExternalOrganizationChangeLogResponsible responsible, IEnumerable<ExternalConnectionAddNewLogEntryInput> logInputs)
        {
            var domainEvent = new ExternalOrganizationConnectionUpdated(organization,
                Mock.Of<IExternalOrganizationalHierarchyConnection>(x =>
                    x.Origin == organizationUnitOrigin),
                new ExternalConnectionAddNewLogInput(A<int>(), responsible,
                    A<DateTime>(), logInputs));
            return domainEvent;
        }
    }
}
