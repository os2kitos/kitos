using System;
using Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping;
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
            var ownerResourceUuid = A<Guid>();

            //Act
            var parameters = _sut.FromImmediatePOST(request, ownerResourceUuid, ownerResourceType);

            //Assert
            AssertImmediateRequest(ownerResourceUuid, request, parameters, ownerResourceType);
        }

        [Theory]
        [InlineData(OwnerResourceType.DataProcessingRegistration)]
        [InlineData(OwnerResourceType.ItContract)]
        [InlineData(OwnerResourceType.ItSystemUsage)]
        public void Can_Map_FromScheduledPOST(OwnerResourceType ownerResourceType)
        {
            //Arrange
            var request = A<ScheduledNotificationWriteRequestDTO>();
            var ownerResourceUuid = A<Guid>();

            //Act
            var parameters = _sut.FromScheduledPOST(request, ownerResourceUuid, ownerResourceType);

            //Assert
            AssertScheduledRequest(ownerResourceUuid, request, parameters, ownerResourceType);
        }

        [Theory]
        [InlineData(OwnerResourceType.DataProcessingRegistration)]
        [InlineData(OwnerResourceType.ItContract)]
        [InlineData(OwnerResourceType.ItSystemUsage)]
        public void Can_Map_FromScheduledPUT(OwnerResourceType ownerResourceType)
        {
            //Arrange
            var request = A<UpdateScheduledNotificationWriteRequestDTO>();
            var ownerResourceUuid = A<Guid>();

            //Act
            var parameters = _sut.FromScheduledPUT(request, ownerResourceUuid, ownerResourceType);

            //Assert
            AssertUpdateScheduledRequest(ownerResourceUuid, request, parameters, ownerResourceType);
        }

        private static void AssertScheduledRequest(Guid ownerResourceUuid, ScheduledNotificationWriteRequestDTO request,
            CreateScheduledNotificationModificationParameters parameters, OwnerResourceType ownerResourceType)
        {
            AssertImmediateRequest(ownerResourceUuid, request, parameters, ownerResourceType);

            Assert.Equal(request.FromDate, parameters.FromDate);
            Assert.Equal(request.RepetitionFrequency.ToScheduling(), parameters.RepetitionFrequency);
        }

        private static void AssertUpdateScheduledRequest(Guid ownerResourceUuid, UpdateScheduledNotificationWriteRequestDTO request,
            UpdateScheduledNotificationModificationParameters parameters, OwnerResourceType ownerResourceType)
        {
            AssertImmediateRequest(ownerResourceUuid, request, parameters, ownerResourceType);

            Assert.Equal(request.Name, parameters.Name);
            Assert.Equal(request.ToDate, parameters.ToDate);
        }

        private static void AssertImmediateRequest<TDTO, TParameters>(Guid ownerResourceUuid, TDTO request, TParameters parameters, OwnerResourceType ownerResourceType)
        where TDTO: class, IHasBaseWriteProperties
        where TParameters: class, IHasBaseNotificationPropertiesParameters
        {
            Assert.Equal(request.BaseProperties.Body, parameters.BaseProperties.Body);
            Assert.Equal(request.BaseProperties.Subject, parameters.BaseProperties.Subject);
            Assert.Equal(ownerResourceUuid, parameters.BaseProperties.OwnerResourceUuid);
            Assert.Equal(ownerResourceType, parameters.BaseProperties.Type.ToOwnerResourceType());
            AssertRootRecipients(request.BaseProperties.Ccs, parameters.BaseProperties.Ccs);
            AssertRootRecipients(request.BaseProperties.Receivers, parameters.BaseProperties.Receivers);
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
