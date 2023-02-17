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
        public async Task Can_Create_ImmediateNotification()
        {
            var ownerResourceType = A<OwnerResourceType>();
            var (relationUuid, organization) = await SetupDataAsync(ownerResourceType);

            var body = CreateBaseNotificationWriteRequest<ImmediateNotificationWriteRequestDTO>(relationUuid, ownerResourceType);

            var response = await NotificationV2Helper.CreateImmediateNotificationAsync(ownerResourceType, organization.Uuid, body);
            Assert.NotNull(response);

            var notification = await NotificationV2Helper.GetNotificationByUuid(ownerResourceType, response.Uuid);
            AssertBaseNotification(body, notification, NotificationSendType.Immediate, notification.Uuid);
        }

        [Fact]
        public async Task Can_Create_Update_Deactivate_And_Delete_ScheduledNotification()
        {
            var ownerResourceType = A<OwnerResourceType>();
            var (relationUuid, organization) = await SetupDataAsync(ownerResourceType);

            var body = CreateBaseScheduledNotificationWriteRequest<ScheduledNotificationWriteRequestDTO>(relationUuid, ownerResourceType);

            body.FromDate = DateTime.UtcNow.AddDays(A<int>());
            //Make sure ToDate is larger than FromDate
            body.ToDate = body.FromDate.AddDays(1);
            body.RepetitionFrequency = A<RepetitionFrequencyOptions>();

            var response = await NotificationV2Helper.CreateScheduledNotificationAsync(ownerResourceType, organization.Uuid, body);
            Assert.NotNull(response);

            var notification = await NotificationV2Helper.GetNotificationByUuid(ownerResourceType, response.Uuid);
            AssertScheduledNotification(expected: body, actual: notification, NotificationSendType.Repeat, notification.Uuid);

            var updateBody = CreateBaseScheduledNotificationWriteRequest<UpdateScheduledNotificationWriteRequestDTO>(relationUuid, ownerResourceType);
            
            var updateResponse = await NotificationV2Helper.UpdateScheduledNotificationAsync(ownerResourceType, notification.Uuid, updateBody);
            Assert.NotNull(updateResponse);

            var updatedNotification = await NotificationV2Helper.GetNotificationByUuid(ownerResourceType, notification.Uuid);
            AssertUpdateScheduledNotification(updateBody, updatedNotification, NotificationSendType.Repeat, notification.Uuid);

            var deactivateResponse = await NotificationV2Helper.DeactivateNotificationAsync(ownerResourceType, notification.Uuid);
            Assert.NotNull(deactivateResponse);
            Assert.False(deactivateResponse.Active);

            await NotificationV2Helper.DeleteNotificationAsync(ownerResourceType, notification.Uuid);
            using var getDeletedNotificationResponse = await NotificationV2Helper.SendGetNotificationByUuid(ownerResourceType, notification.Uuid);
            Assert.Equal(HttpStatusCode.NotFound, getDeletedNotificationResponse.StatusCode);
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

        private async Task<(Guid relationUuid, OrganizationDTO organization)> SetupDataAsync(OwnerResourceType ownerResourceType)
        {
            var relationUuid = await SetupRelatedResourceAsync(ownerResourceType);
            var organization = await SetupOrganizationAsync();
            return (relationUuid, organization);
        }

        private async Task<Guid> SetupRelatedResourceAsync(OwnerResourceType ownerResourceType)
        {
            switch (ownerResourceType)
            {
                case OwnerResourceType.ItContract:
                    var result = await ItContractHelper.CreateContract(A<string>(), TestEnvironment.DefaultOrganizationId);
                    return result.Uuid;
                case OwnerResourceType.ItSystemUsage:
                    var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
                    var usageResult = ItSystemUsageHelper.CreateItSystemUsageAsync(new ItSystemUsage {ItSystemId = system.Id, OrganizationId = TestEnvironment.DefaultOrganizationId});
                    return usageResult.Uuid;
                case OwnerResourceType.DataProcessingRegistration:
                    var dpr = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
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
