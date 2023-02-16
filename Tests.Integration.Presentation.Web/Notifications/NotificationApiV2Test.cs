using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
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
            var ownerResourceType = OwnerResourceType.ItContract;//A<OwnerResourceType>();
            var (relationUuid, organization) = await SetupDataAsync(ownerResourceType);

            var body = new ImmediateNotificationWriteRequestDTO
            {
                Body = A<string>(),
                OwnerResourceUuid = relationUuid,
                Subject = A<string>(),
                Ccs = new RecipientWriteRequestDTO
                {
                    EmailRecipients = new List<EmailRecipientWriteRequestDTO>()
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
                    EmailRecipients = new List<EmailRecipientWriteRequestDTO>()
                    {
                        new() {Email = CreateEmail()}
                    }
                }
            };

            var response = await NotificationV2Helper.CreateNotificationAsync(ownerResourceType, organization.Uuid, body);

            Assert.NotNull(response);
            var notification = await NotificationV2Helper.GetNotificationByUuid(ownerResourceType, response.Uuid);
            Assert.Equal(body.Body, notification.Body);
            Assert.Equal(body.Subject, notification.Subject);
            Assert.Equal(body.OwnerResourceUuid, notification.OwnerResourceUuid);
        }

        private async Task<(Guid relationUuid, OrganizationDTO organization)> SetupDataAsync(OwnerResourceType ownerResourceType)
        {
            var relationUuid = await SetupRelatedResourceAsync(ownerResourceType);
            var organization = await SetupOrganizationAsync();
            return (relationUuid, organization);
        }

        private async Task<(User user, string token)> CreateApiUser(OrganizationDTO organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
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
                    var usageResult = ItSystemUsageHelper.CreateItSystemUsageAsync(new ItSystemUsage(){OrganizationId = TestEnvironment.DefaultOrganizationId});
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
            ItContractRole roleEntity = new ItContractRole();
            switch (ownerResourceType)
            {
                case OwnerResourceType.ItContract:
                    roleEntity = new ItContractRole {Name = CreateName()};
                    break;
                case OwnerResourceType.ItSystemUsage:
                    break;
                case OwnerResourceType.DataProcessingRegistration:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ownerResourceType), ownerResourceType, null);
            }

            InsertRole(roleEntity);
            return roleEntity;
        }

        private static IRoleEntity InsertRole(ItContractRole role)
        {
            DatabaseAccess.MutateEntitySet<ItContractRole>(repository =>
            {
                repository.Insert(role);
            });

            return role;
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
