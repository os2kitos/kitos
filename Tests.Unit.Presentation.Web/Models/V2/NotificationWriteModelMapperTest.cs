using Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.Notification.Write;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Types.Notifications;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class NotificationWriteModelMapperTest : WithAutoFixture
    {
        private readonly NotificationWriteModelMapper _sut = new ();

        [Theory]
        [InlineData(OwnerResourceType.DataProcessingRegistration)]
        [InlineData(OwnerResourceType.ItContract)]
        [InlineData(OwnerResourceType.ItSystemUsage)]
        public void Can_Map_FromImmediatePOST(OwnerResourceType ownerResourceType)
        {
            //Arrange
            var request = A<ImmediateNotificationWriteRequestDTO>();

            //Act
            var parameters = _sut.FromImmediatePOST(request, ownerResourceType);

            //Assert
            AssertImmediateRequest(request, parameters, ownerResourceType);
        }

        [Theory]
        [InlineData(OwnerResourceType.DataProcessingRegistration)]
        [InlineData(OwnerResourceType.ItContract)]
        [InlineData(OwnerResourceType.ItSystemUsage)]
        public void Can_Map_FromScheduledPOST(OwnerResourceType ownerResourceType)
        {
            //Arrange
            var request = A<ScheduledNotificationWriteRequestDTO>();

            //Act
            var parameters = _sut.FromScheduledPOST(request, ownerResourceType);

            //Assert
            AssertScheduledRequest(request, parameters, ownerResourceType);
        }

        [Theory]
        [InlineData(OwnerResourceType.DataProcessingRegistration)]
        [InlineData(OwnerResourceType.ItContract)]
        [InlineData(OwnerResourceType.ItSystemUsage)]
        public void Can_Map_FromScheduledPUT(OwnerResourceType ownerResourceType)
        {
            //Arrange
            var request = A<UpdateScheduledNotificationWriteRequestDTO>();

            //Act
            var parameters = _sut.FromScheduledPUT(request, ownerResourceType);

            //Assert
            AssertUpdateScheduledRequest(request, parameters, ownerResourceType);
        }

        private static void AssertScheduledRequest(ScheduledNotificationWriteRequestDTO request,
            ScheduledNotificationModificationParameters parameters, OwnerResourceType ownerResourceType)
        {
            AssertImmediateRequest(request, parameters, ownerResourceType);

            Assert.Equal(request.FromDate, parameters.FromDate);
            Assert.Equal(request.RepetitionFrequency.ToScheduling(), parameters.RepetitionFrequency);
        }

        private static void AssertUpdateScheduledRequest(UpdateScheduledNotificationWriteRequestDTO request,
            UpdateScheduledNotificationModificationParameters parameters, OwnerResourceType ownerResourceType)
        {
            AssertImmediateRequest(request, parameters, ownerResourceType);

            Assert.Equal(request.Name, parameters.Name);
            Assert.Equal(request.ToDate, parameters.ToDate);
        }

        private static void AssertImmediateRequest(ImmediateNotificationWriteRequestDTO request, ImmediateNotificationModificationParameters parameters, OwnerResourceType ownerResourceType)
        {
            Assert.Equal(request.Body, parameters.Body);
            Assert.Equal(request.Subject, parameters.Subject);
            Assert.Equal(request.OwnerResourceUuid, parameters.OwnerResourceUuid);
            Assert.Equal(ownerResourceType, parameters.Type.ToOwnerResourceType());
            AssertRootRecipients(request.Ccs, parameters.Ccs);
            AssertRootRecipients(request.Receivers, parameters.Receivers);
        }

        private static void AssertRootRecipients(RecipientWriteRequestDTO request, RootRecipientModificationParameters parameters)
        {
            AssertEmailRecipients(request.EmailRecipients, parameters.EmailRecipients);
            AssertRoleRecipients(request.RoleRecipients, parameters.RoleRecipients);
        }

        private static void AssertEmailRecipients(IEnumerable<EmailRecipientWriteRequestDTO> request,
            IEnumerable<EmailRecipientModificationParameters> parameters)
        {
            var parametersList = parameters.ToList();
            foreach (var dto in request)
            {
                Assert.Single(parametersList, x => x.Email == dto.Email);
            }
        }
        private static void AssertRoleRecipients(IEnumerable<RoleRecipientWriteRequestDTO> request,
            IEnumerable<RoleRecipientModificationParameters> parameters)
        {
            var parametersList = parameters.ToList();
            foreach (var dto in request)
            {
                Assert.Single(parametersList, x => x.RoleUuid == dto.RoleUuid);
            }
        }
    }
}
