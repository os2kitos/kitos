using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Notification;
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

            var body = CreateImmediateNotificationWriteRequest(ownerResourceType);

            //Act
            var response = await NotificationV2Helper.CreateImmediateNotificationAsync(ownerResourceType, relationUuid, body, cookie);

            //Assert
            Assert.NotNull(response);

            var notification = await NotificationV2Helper.GetNotificationByUuid(ownerResourceType, relationUuid, response.Uuid, cookie);
            AssertBaseNotification(body, notification, NotificationSendType.Immediate, relationUuid, notification.Uuid);
        }

        [Fact]
        public async Task Can_Create_Get_Update_Deactivate_And_Delete_ScheduledNotification()
        {
            //Arrange
            var ownerResourceType = A<OwnerResourceType>();
            var (relationUuid, _, cookie) = await CreatePrerequisitesAsync(ownerResourceType);

            var body = CreateScheduledNotificationWriteRequest(ownerResourceType);

            //Act
            var response = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, relationUuid, body, cookie);

            //Assert
            Assert.NotNull(response);

            var notification = await NotificationV2Helper.GetNotificationByUuid(ownerResourceType, relationUuid, response.Uuid, cookie);
            AssertScheduledNotification(expected: body, actual: notification, NotificationSendType.Repeat, relationUuid, notification.Uuid);

            //Arrange - update
            var updateBody = CreateUpdateScheduledNotificationWriteRequest(ownerResourceType);

            //Act - update
            var updateResponse = await NotificationV2Helper.UpdateScheduledNotificationAsync(ownerResourceType, relationUuid, notification.Uuid, updateBody, cookie);

            //Assert - update
            Assert.NotNull(updateResponse);

            var updatedNotification = await NotificationV2Helper.GetNotificationByUuid(ownerResourceType, relationUuid, notification.Uuid, cookie);
            AssertUpdateScheduledNotification(updateBody, updatedNotification, NotificationSendType.Repeat, relationUuid, notification.Uuid);

            //Act - deactivate
            var deactivateResponse = await NotificationV2Helper.DeactivateNotificationAsync(ownerResourceType, relationUuid, notification.Uuid, cookie);

            //Assert - deactivate
            Assert.NotNull(deactivateResponse);
            Assert.False(deactivateResponse.Active);

            //Act - delete
            await NotificationV2Helper.DeleteNotificationAsync(ownerResourceType, relationUuid, notification.Uuid, cookie);

            //Assert - delete
            using var getDeletedNotificationResponse = await NotificationV2Helper.SendGetNotificationByUuid(ownerResourceType, relationUuid, notification.Uuid, cookie);
            Assert.Equal(HttpStatusCode.NotFound, getDeletedNotificationResponse.StatusCode);
        }

        [Fact]
        public async Task Can_GetAll_Notifications()
        {
            //Arrange
            var ownerResourceType = A<OwnerResourceType>();
            var (relationUuid, organization, cookie) = await CreatePrerequisitesAsync(ownerResourceType);

            var notification1 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, relationUuid, CreateScheduledNotificationWriteRequest(ownerResourceType), cookie);
            var notification2 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, relationUuid,  CreateScheduledNotificationWriteRequest(ownerResourceType), cookie);
            var notification3 = await NotificationV2Helper.CreateImmediateNotificationAsync(ownerResourceType, relationUuid,  CreateImmediateNotificationWriteRequest(ownerResourceType), cookie);
            var notification4 = await NotificationV2Helper.CreateImmediateNotificationAsync(ownerResourceType, relationUuid, CreateImmediateNotificationWriteRequest(ownerResourceType), cookie);

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

            var notification1 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, relationUuid,  CreateScheduledNotificationWriteRequest(ownerResourceType), cookie);
            var notification2 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, relationUuid,  CreateScheduledNotificationWriteRequest(ownerResourceType), cookie);
            var notification3 = await NotificationV2Helper.CreateImmediateNotificationAsync(ownerResourceType, relationUuid,  CreateImmediateNotificationWriteRequest(ownerResourceType), cookie);
            var notification4 = await NotificationV2Helper.CreateImmediateNotificationAsync(ownerResourceType, relationUuid, CreateImmediateNotificationWriteRequest(ownerResourceType), cookie);

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
        public async Task Can_GetAll_Notifications_Filtered_By_OwnerResourceUuid()
        {
            //Arrange
            var ownerResourceType = A<OwnerResourceType>();
            var (relationUuid, organization, cookie) = await CreatePrerequisitesAsync(ownerResourceType);
            var relation2Uuid = await SetupRelatedResourceAsync(ownerResourceType, organization.Id);

            var notificationRequest1 = CreateScheduledNotificationWriteRequest(ownerResourceType);
            var notificationRequest2 = CreateScheduledNotificationWriteRequest(ownerResourceType);
            var notificationRequest3 = CreateScheduledNotificationWriteRequest(ownerResourceType);
            
            var notification1 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, relationUuid, notificationRequest1, cookie);
            var notification2 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, relationUuid, notificationRequest2, cookie);
            await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, relation2Uuid, notificationRequest3, cookie);

            //Act
            var response = await NotificationV2Helper.GetNotificationsAsync(ownerResourceType, organization.Uuid, ownerResourceUuid: relationUuid, userCookie: cookie);
            var notifications = response.ToList();

            //Assert
            Assert.Equal(2, notifications.Count);
            AssertNotificationList(notification1, notifications);
            AssertNotificationList(notification2, notifications);
        }

        [Fact]
        public async Task Can_GetAll_Notifications_Filtered_By_Active()
        {
            //Arrange
            var ownerResourceType = A<OwnerResourceType>();
            var (relationUuid, organization, cookie) = await CreatePrerequisitesAsync(ownerResourceType);

            var notificationRequest1 = CreateScheduledNotificationWriteRequest(ownerResourceType);
            var notificationRequest2 = CreateScheduledNotificationWriteRequest(ownerResourceType);

            var notification1 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, relationUuid, notificationRequest1, cookie);
            var notification2 = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, relationUuid, notificationRequest2, cookie);

            await NotificationV2Helper.DeactivateNotificationAsync(ownerResourceType, relationUuid, notification2.Uuid, cookie);

            //Act
            var response = await NotificationV2Helper.GetNotificationsAsync(ownerResourceType, organization.Uuid, onlyActive: true, userCookie: cookie);
            var notifications = response.ToList();

            //Assert
            Assert.Single(notifications);
            AssertNotificationList(notification1, notifications);
        }

        [Fact]
        public async Task Can_GetPermissions()
        {
            //Arrange
            var ownerResourceType = A<OwnerResourceType>();
            var (ownerResourceUuid, _, cookie) = await CreatePrerequisitesAsync(ownerResourceType);

            var notificationRequest1 = CreateScheduledNotificationWriteRequest(ownerResourceType);

            var notification = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, ownerResourceUuid, notificationRequest1, cookie);

            //Act
            var response = await NotificationV2Helper.GetPermissionsAsync(ownerResourceType, ownerResourceUuid, notification.Uuid, userCookie: cookie);

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
            Assert.Equal(expected.OwnerResource.Uuid, actual.OwnerResource.Uuid);
            AssertRecipients(expected, actual);
        }

        private static void AssertScheduledNotification(ScheduledNotificationWriteRequestDTO expected, NotificationResponseDTO actual, NotificationSendType notificationType, Guid relationUuid, Guid notificationUuid)
        {
            Assert.Equal(expected.RepetitionFrequency, actual.RepetitionFrequency);
            Assert.Equal(expected.ToDate, actual.ToDate);
            AssertUpdateScheduledNotification(expected, actual, notificationType, relationUuid, notificationUuid);
        }

        private static void AssertUpdateScheduledNotification<T>(T expected, NotificationResponseDTO actual, NotificationSendType notificationType, Guid relationUuid, Guid notificationUuid) where T : class, IHasBaseWriteProperties, IHasName, IHasToDate
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.ToDate, actual.ToDate);
            AssertBaseNotification(expected, actual, notificationType, relationUuid, notificationUuid);
        }

        private static void AssertBaseNotification<T>(T expected, NotificationResponseDTO actual, NotificationSendType notificationType, Guid relationUuid, Guid notificationUuid) where T: class, IHasBaseWriteProperties
        {
            Assert.Equal(notificationUuid, actual.Uuid);
            Assert.Equal(notificationType, actual.NotificationType);
            Assert.Equal(expected.BaseProperties.Body, actual.Body);
            Assert.Equal(expected.BaseProperties.Subject, actual.Subject);
            Assert.Equal(relationUuid, actual.OwnerResource.Uuid);
            AssertRecipients(expected, actual);
        }

        private static void AssertRecipients<T>(T expected, NotificationResponseDTO actual) where T : class, IHasBaseWriteProperties
        {
            if (expected?.BaseProperties.Ccs != null)
            {
                AssertEmailRecipients(expected.BaseProperties.Ccs.EmailRecipients, actual.CCs.EmailRecipients);
                AssertRoleRecipients(expected.BaseProperties.Ccs.RoleRecipients, actual.CCs.RoleRecipients);
            }
            else
            {
                Assert.Null(actual?.CCs);
            }

            if (expected?.BaseProperties.Receivers != null)
            {
                AssertEmailRecipients(expected.BaseProperties.Receivers.EmailRecipients, actual.Receivers.EmailRecipients);
                AssertRoleRecipients(expected.BaseProperties.Receivers.RoleRecipients, actual.Receivers.RoleRecipients);
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

        private ImmediateNotificationWriteRequestDTO CreateImmediateNotificationWriteRequest(OwnerResourceType ownerResourceType)
        {
            var request = new ImmediateNotificationWriteRequestDTO();
            CreateBaseProperties(request, ownerResourceType);

            return request;
        }

        private ScheduledNotificationWriteRequestDTO CreateScheduledNotificationWriteRequest(OwnerResourceType ownerResourceType)
        {
            var request = new ScheduledNotificationWriteRequestDTO();
            CreateBaseScheduledProperties(request, ownerResourceType);

            request.FromDate = DateTime.UtcNow.AddDays(A<int>());
            //Make sure ToDate is larger than FromDate
            request.ToDate = request.FromDate.AddDays(A<int>());
            request.RepetitionFrequency = A<RepetitionFrequencyOptions>();

            return request;
        }

        private UpdateScheduledNotificationWriteRequestDTO CreateUpdateScheduledNotificationWriteRequest(OwnerResourceType ownerResourceType)
        {
            var request = new UpdateScheduledNotificationWriteRequestDTO();
            CreateBaseScheduledProperties(request, ownerResourceType);

            return request;
        }

        private void CreateBaseScheduledProperties<T>(T request, OwnerResourceType ownerResourceType) where T : class, IHasBaseWriteProperties, IHasName, IHasToDate, new()
        {
            CreateBaseProperties(request, ownerResourceType);
            request.Name = A<string>();
            request.ToDate = A<DateTime>();
        }

        private void CreateBaseProperties<TModel>(TModel model, OwnerResourceType ownerResourceType) where TModel : class, IHasBaseWriteProperties, new()
        {
            model.BaseProperties = new BaseNotificationPropertiesWriteRequestDTO
                {
                    Subject = A<string>(),
                    Body = A<string>(),
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
                    var usageResult = ItSystemUsageHelper.CreateItSystemUsage(new ItSystemUsage { ItSystemId = system.Id, OrganizationId = organizationId });
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
                    var contractRoleEntity = new ItContractRole { Name = CreateName() };
                    InsertContractRole(contractRoleEntity);
                    return contractRoleEntity;
                case OwnerResourceType.ItSystemUsage:
                    var systemRoleEntity = new ItSystemRole { Name = CreateName() };
                    InsertSystemRole(systemRoleEntity);
                    return systemRoleEntity;
                case OwnerResourceType.DataProcessingRegistration:
                    var dprRoleEntity = new DataProcessingRegistrationRole { Name = CreateName() };
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
