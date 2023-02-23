/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;
using Presentation.Web.Models.API.V2.Types.Notifications;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Internal.Notifications;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Notifications
{
    public class NotificationApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task Can_Create_And_Get_ImmediateNotification()
        {
            //Arrange
            var ownerResourceType = A<OwnerResourceType>();
            var (relationUuid, _, cookie) = await CreatePrerequisitesAsync(ownerResourceType);

            var body = CreateBaseNotificationWriteRequest<ImmediateNotificationWriteRequestDTO>(relationUuid, ownerResourceType);

            //Act
            var response = await NotificationV2Helper.CreateImmediateNotificationAsync(ownerResourceType, body, cookie);

            //Assert
            Assert.NotNull(response);

            var notification = await NotificationV2Helper.GetNotificationByUuid(ownerResourceType, response.Uuid, cookie);
            AssertBaseNotification(body, notification, NotificationSendType.Immediate, notification.Uuid);
        }

        [Fact]
        public async Task Can_Create_Get_Update_Deactivate_And_Delete_ScheduledNotification()
        {
            //Arrange
            var ownerResourceType = A<OwnerResourceType>();
            var (relationUuid, _, cookie) = await CreatePrerequisitesAsync(ownerResourceType);

            var body = CreateScheduledNotificationWriteRequest(relationUuid, ownerResourceType);
            
            //Act
            var response = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, body, cookie);

            //Assert
            Assert.NotNull(response);

            var notification = await NotificationV2Helper.GetNotificationByUuid(ownerResourceType, response.Uuid, cookie);
            AssertScheduledNotification(expected: body, actual: notification, NotificationSendType.Repeat, notification.Uuid);

            //Arrange - update
            var updateBody = CreateBaseScheduledNotificationWriteRequest<UpdateScheduledNotificationWriteRequestDTO>(relationUuid, ownerResourceType);

            //Act - update
            var updateResponse = await NotificationV2Helper.UpdateScheduledNotificationAsync(ownerResourceType, notification.Uuid, updateBody, cookie);

            //Assert - update
            Assert.NotNull(updateResponse);

            var updatedNotification = await NotificationV2Helper.GetNotificationByUuid(ownerResourceType, notification.Uuid, cookie);
            AssertUpdateScheduledNotification(updateBody, updatedNotification, NotificationSendType.Repeat, notification.Uuid);

            //Act - deactivate
            var deactivateResponse = await NotificationV2Helper.DeactivateNotificationAsync(ownerResourceType, notification.Uuid, cookie);

            //Assert - deactivate
            Assert.NotNull(deactivateResponse);
            Assert.False(deactivateResponse.Active);

            //Act - delete
            await NotificationV2Helper.DeleteNotificationAsync(ownerResourceType, notification.Uuid, cookie);

            //Assert - delete
            using var getDeletedNotificationResponse = await NotificationV2Helper.SendGetNotificationByUuid(ownerResourceType, notification.Uuid, cookie);
            Assert.Equal(HttpStatusCode.NotFound, getDeletedNotificationResponse.StatusCode);
        }

        [Fact]
        public async Task Can_GetAll_Notifications()
        {
            //Arrange
            var ownerResourceType = A<OwnerResourceType>();
            var (relationUuid, organization, cookie) = await CreatePrerequisitesAsync(ownerResourceType);

            var notification1 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, CreateScheduledNotificationWriteRequest(relationUuid, ownerResourceType), cookie);
            var notification2 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, CreateScheduledNotificationWriteRequest(relationUuid, ownerResourceType), cookie);
            var notification3 = await NotificationV2Helper.CreateImmediateNotificationAsync(ownerResourceType, CreateBaseNotificationWriteRequest<ImmediateNotificationWriteRequestDTO>(relationUuid, ownerResourceType), cookie);
            var notification4 = await NotificationV2Helper.CreateImmediateNotificationAsync(ownerResourceType, CreateBaseNotificationWriteRequest<ImmediateNotificationWriteRequestDTO>(relationUuid, ownerResourceType), cookie);

            //Act
            var response = await NotificationV2Helper.GetNotificationsAsync(ownerResourceType, organization.Uuid, userCookie: cookie);
            var notifications = response.ToList();

            //Assert
            Assert.Equal(4, notifications.Count);
            AssertNotificationList(notification1, notifications);
            AssertNotificationList(notification2, notifications);
            AssertNotificationList(notification3, notifications);
            AssertNotificationList(notification4, notifications);
        }

        [Fact]
        public async Task Can_GetAll_Notifications_With_Paging()
        {
            //Arrange
            var ownerResourceType = A<OwnerResourceType>();
            var (relationUuid, organization, cookie) = await CreatePrerequisitesAsync(ownerResourceType);

            var notification1 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, CreateScheduledNotificationWriteRequest(relationUuid, ownerResourceType), cookie);
            var notification2 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, CreateScheduledNotificationWriteRequest(relationUuid, ownerResourceType), cookie);
            var notification3 = await NotificationV2Helper.CreateImmediateNotificationAsync(ownerResourceType, CreateBaseNotificationWriteRequest<ImmediateNotificationWriteRequestDTO>(relationUuid, ownerResourceType), cookie);
            var notification4 = await NotificationV2Helper.CreateImmediateNotificationAsync(ownerResourceType, CreateBaseNotificationWriteRequest<ImmediateNotificationWriteRequestDTO>(relationUuid, ownerResourceType), cookie);

            //Act
            var responsePage1 = await NotificationV2Helper.GetNotificationsAsync(ownerResourceType, organization.Uuid, page: 0, pageSize: 2, userCookie: cookie);
            var responsePage2 = await NotificationV2Helper.GetNotificationsAsync(ownerResourceType, organization.Uuid, page: 1, pageSize: 2, userCookie: cookie);
            var notificationsPage1 = responsePage1.ToList();
            var notificationsPage2 = responsePage2.ToList();

            //Assert
            Assert.Equal(2, notificationsPage1.Count);
            Assert.Equal(2, notificationsPage2.Count);

            AssertNotificationList(notification1, notificationsPage1);
            AssertNotificationList(notification2, notificationsPage1);
            AssertNotificationList(notification3, notificationsPage2);
            AssertNotificationList(notification4, notificationsPage2);
        }

        [Fact]
        public async Task Can_GetAll_Notifications_Filtered_By_FromDate()
        {
            //Arrange
            var ownerResourceType = A<OwnerResourceType>();
            var (relationUuid, organization, cookie) = await CreatePrerequisitesAsync(ownerResourceType);

            var notificationRequest1 = CreateScheduledNotificationWriteRequest(relationUuid, ownerResourceType);
            var notificationRequest2 = CreateScheduledNotificationWriteRequest(relationUuid, ownerResourceType);
            var notificationRequest3 = CreateScheduledNotificationWriteRequest(relationUuid, ownerResourceType);

            notificationRequest2.FromDate = notificationRequest1.FromDate.AddDays(-1);
            notificationRequest3.FromDate = notificationRequest1.FromDate.AddDays(1);

            var notification1 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, notificationRequest1, cookie);
            var notification2 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, notificationRequest2, cookie);
            var notification3 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, notificationRequest3, cookie);

            //Act
            var response = await NotificationV2Helper.GetNotificationsAsync(ownerResourceType, organization.Uuid, userCookie: cookie);
            var notifications = response.ToList();

            //Assert
            Assert.Equal(3, notifications.Count);
            AssertNotificationList(notification1, notifications);
            AssertNotificationList(notification2, notifications);
            AssertNotificationList(notification3, notifications);
        }

        [Fact]
        public async Task Can_GetAccessRights()
        {
            //Arrange
            var ownerResourceType = A<OwnerResourceType>();
            var (ownerResourceUuid, _, cookie) = await CreatePrerequisitesAsync(ownerResourceType);

            var notificationRequest1 = CreateScheduledNotificationWriteRequest(ownerResourceUuid, ownerResourceType);

            var notification = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, notificationRequest1, cookie);

            //Act
            var response = await NotificationV2Helper.GetAccessRightsAsync(ownerResourceType, notification.Uuid, ownerResourceUuid, userCookie: cookie);

            //Assert
            Assert.True(response.Read);
            Assert.True(response.Modify);
            Assert.True(response.Deactivate);
            Assert.False(response.Delete);
        }

        private static void AssertNotificationList(NotificationResponseDTO expected, IEnumerable<NotificationResponseDTO> actual)
        {
            var dto = Assert.Single(actual, x => x.Uuid == expected.Uuid);
            AssertNotificationResponseDTO(expected, dto);
        }

        private static void AssertNotificationResponseDTO(NotificationResponseDTO expected,
            NotificationResponseDTO actual)
        {
            Assert.Equal(expected.RepetitionFrequency, actual.RepetitionFrequency);
            Assert.Equal(expected.ToDate, actual.ToDate);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.ToDate, actual.ToDate);
            Assert.Equal(expected.Uuid, actual.Uuid);
            Assert.Equal(expected.NotificationType, actual.NotificationType);
            Assert.Equal(expected.Body, actual.Body);
            Assert.Equal(expected.Subject, actual.Subject);
            Assert.Equal(expected.OwnerResourceUuid, actual.OwnerResourceUuid);
            AssertRecipients(expected, actual);
        }

        private static void AssertScheduledNotification(ScheduledNotificationWriteRequestDTO expected, NotificationResponseDTO actual, NotificationSendType notificationType, Guid notificationUuid)
        {
            Assert.Equal(expected.RepetitionFrequency, actual.RepetitionFrequency);
            Assert.Equal(expected.ToDate, actual.ToDate);
            AssertUpdateScheduledNotification(expected, actual, notificationType, notificationUuid);
        }

        private static void AssertUpdateScheduledNotification(UpdateScheduledNotificationWriteRequestDTO expected, NotificationResponseDTO actual, NotificationSendType notificationType, Guid notificationUuid)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.ToDate, actual.ToDate);
            AssertBaseNotification(expected, actual, notificationType, notificationUuid);
        }

        private static void AssertBaseNotification(ImmediateNotificationWriteRequestDTO expected, NotificationResponseDTO actual, NotificationSendType notificationType, Guid notificationUuid)
        {
            Assert.Equal(notificationUuid, actual.Uuid);
            Assert.Equal(notificationType, actual.NotificationType);
            Assert.Equal(expected.Body, actual.Body);
            Assert.Equal(expected.Subject, actual.Subject);
            Assert.Equal(expected.OwnerResourceUuid, actual.OwnerResourceUuid);
            AssertRecipients(expected, actual);
        }

        private static void AssertRecipients(ImmediateNotificationWriteRequestDTO expected, NotificationResponseDTO actual)
        {
            if (expected?.Ccs != null)
            {
                AssertEmailRecipients(expected.Ccs.EmailRecipients, actual.CCs.EmailRecipients);
                AssertRoleRecipients(expected.Ccs.RoleRecipients, actual.CCs.RoleRecipients);
            }
            else
            {
                Assert.Null(actual?.CCs);
            }

            if (expected?.Receivers != null)
            {
                AssertEmailRecipients(expected.Receivers.EmailRecipients, actual.Receivers.EmailRecipients);
                AssertRoleRecipients(expected.Receivers.RoleRecipients, actual.Receivers.RoleRecipients);
            }
            else
            {
                Assert.Null(actual?.Receivers);
            }
        }

        private static void AssertRoleRecipients(IEnumerable<RoleRecipientWriteRequestDTO> expected, IEnumerable<RoleRecipientResponseDTO> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            var actualList = actual.ToList();

            foreach (var role in expected)
            {
                Assert.Single(actualList, x => x.Role.Uuid == role.RoleUuid);
            }
        }

        private static void AssertEmailRecipients(IEnumerable<EmailRecipientWriteRequestDTO> expected, IEnumerable<EmailRecipientResponseDTO> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            var actualList = actual.ToList();
            foreach (var email in expected)
            {
                Assert.Single(actualList, x => x.Email == email.Email);
            }
        }

        private static void AssertRecipients(NotificationResponseDTO expected, NotificationResponseDTO actual)
        {
            if (expected?.CCs != null)
            {
                AssertEmailRecipients(expected.CCs.EmailRecipients, actual.CCs.EmailRecipients);
                AssertRoleRecipients(expected.CCs.RoleRecipients, actual.CCs.RoleRecipients);
            }
            else
            {
                Assert.Null(actual?.CCs);
            }

            if (expected?.Receivers != null)
            {
                AssertEmailRecipients(expected.Receivers.EmailRecipients, actual.Receivers.EmailRecipients);
                AssertRoleRecipients(expected.Receivers.RoleRecipients, actual.Receivers.RoleRecipients);
            }
            else
            {
                Assert.Null(actual?.Receivers);
            }
        }

        private static void AssertRoleRecipients(IEnumerable<RoleRecipientResponseDTO> expected, IEnumerable<RoleRecipientResponseDTO> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            var actualList = actual.ToList();

            foreach (var role in expected)
            {
                Assert.Single(actualList, x => x.Role.Uuid == role.Role.Uuid);
            }
        }

        private static void AssertEmailRecipients(IEnumerable<EmailRecipientResponseDTO> expected, IEnumerable<EmailRecipientResponseDTO> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            var actualList = actual.ToList();
            foreach (var email in expected)
            {
                Assert.Single(actualList, x => x.Email == email.Email);
            }
        }
        
        private ScheduledNotificationWriteRequestDTO CreateScheduledNotificationWriteRequest(Guid ownerResourceUuid, OwnerResourceType ownerResourceType)
        {
            var request = CreateBaseScheduledNotificationWriteRequest<ScheduledNotificationWriteRequestDTO>(ownerResourceUuid, ownerResourceType);

            request.FromDate = DateTime.UtcNow.AddDays(A<int>());
            //Make sure ToDate is larger than FromDate
            request.ToDate = request.FromDate.AddDays(1);
            request.RepetitionFrequency = A<RepetitionFrequencyOptions>();

            return request;
        }
        
        private TResult CreateBaseScheduledNotificationWriteRequest<TResult>(Guid ownerResourceUuid, OwnerResourceType ownerResourceType) where TResult : UpdateScheduledNotificationWriteRequestDTO, new()
        {
            var request = CreateBaseNotificationWriteRequest<TResult>(ownerResourceUuid, ownerResourceType);
            request.Name = A<string>();
            request.ToDate = A<DateTime>();
            return request;
        }

        private TResult CreateBaseNotificationWriteRequest<TResult>(Guid ownerResourceUuid, OwnerResourceType ownerResourceType) where TResult : ImmediateNotificationWriteRequestDTO, new()
        {
            return new TResult
            {
                Subject = A<string>(),
                Body = A<string>(),
                OwnerResourceUuid = ownerResourceUuid,
                Ccs = new RecipientWriteRequestDTO
                {
                    EmailRecipients = new List<EmailRecipientWriteRequestDTO>
                    {
                        new() {Email = CreateEmail()}
                    },
                    RoleRecipients = new List<RoleRecipientWriteRequestDTO>
                    {
                        new() {RoleUuid = CreateNewRole(ownerResourceType).Uuid}
                    }
                },
                Receivers = new RecipientWriteRequestDTO
                {
                    EmailRecipients = new List<EmailRecipientWriteRequestDTO>
                    {
                        new() {Email = CreateEmail()}
                    },
                    RoleRecipients = new List<RoleRecipientWriteRequestDTO>
                    {
                        new() {RoleUuid = CreateNewRole(ownerResourceType).Uuid}
                    }
                }
            };
        }

        private async Task<(Guid relationUuid, OrganizationDTO organization, Cookie cookie)> CreatePrerequisitesAsync(OwnerResourceType ownerResourceType, OrganizationRole role = OrganizationRole.GlobalAdmin)
        {
            var organization = await SetupOrganizationAsync();
            var relationUuid = await SetupRelatedResourceAsync(ownerResourceType, organization.Id);
            var (_, _, cookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), role, organization.Id);
            return (relationUuid, organization, cookie);
        }

        private async Task<Guid> SetupRelatedResourceAsync(OwnerResourceType ownerResourceType, int organizationId)
        {
            switch (ownerResourceType)
            {
                case OwnerResourceType.ItContract:
                    var result = await ItContractHelper.CreateContract(A<string>(), organizationId);
                    return result.Uuid;
                case OwnerResourceType.ItSystemUsage:
                    var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
                    var usageResult = ItSystemUsageHelper.CreateItSystemUsageAsync(new ItSystemUsage {ItSystemId = system.Id, OrganizationId = organizationId });
                    return usageResult.Uuid;
                case OwnerResourceType.DataProcessingRegistration:
                    var dpr = await DataProcessingRegistrationHelper.CreateAsync(organizationId, A<string>());
                    return dpr.Uuid;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ownerResourceType), ownerResourceType, null);
            }
        }

        private async Task<OrganizationDTO> SetupOrganizationAsync()
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, "11224455", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            return organization;
        }

        private IRoleEntity CreateNewRole(OwnerResourceType ownerResourceType)
        {
            switch (ownerResourceType)
            {
                case OwnerResourceType.ItContract:
                    var contractRoleEntity = new ItContractRole {Name = CreateName()};
                    InsertContractRole(contractRoleEntity);
                    return contractRoleEntity;
                case OwnerResourceType.ItSystemUsage:
                    var systemRoleEntity = new ItSystemRole {Name = CreateName()};
                    InsertSystemRole(systemRoleEntity);
                    return systemRoleEntity;
                case OwnerResourceType.DataProcessingRegistration:
                    var dprRoleEntity = new DataProcessingRegistrationRole {Name = CreateName()};
                    InsertDprRole(dprRoleEntity);
                    return dprRoleEntity;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ownerResourceType), ownerResourceType, null);
            }
        }

        private static void InsertContractRole(ItContractRole role)
        {
            DatabaseAccess.MutateEntitySet<ItContractRole>(repository =>
            {
                repository.Insert(role);
            });
        }

        private static void InsertSystemRole(ItSystemRole role)
        {
            DatabaseAccess.MutateEntitySet<ItSystemRole>(repository =>
            {
                repository.Insert(role);
            });
        }

        private static void InsertDprRole(DataProcessingRegistrationRole role)
        {
            DatabaseAccess.MutateEntitySet<DataProcessingRegistrationRole>(repository =>
            {
                repository.Insert(role);
            });
        }

        private string CreateName()
        {
            return nameof(NotificationApiV2Test) + A<string>();
        }

        private string CreateEmail()
        {
            return $"{A<string>()}@test.dk";
        }
    }
}
*/