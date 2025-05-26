using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Tracking;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Xunit;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;

namespace Tests.Integration.Presentation.Web.Deltas.V2
{
    [Collection(nameof(SequentialTestGroup))]
    public class DeltaFeedApiV2Test : BaseTest
    {
        public DeltaFeedApiV2Test()
        {
            //Reset the event stream to control expectations
            DatabaseAccess.MutateEntitySet<LifeCycleTrackingEvent>(repository =>
           {
               var allIds = repository.AsQueryable().Select(x => x.Id).ToList();
               foreach (var id in allIds)
               {
                   repository.DeleteByKey(id);
               }
           });
        }

        [Fact]
        public async Task GetDeletedItems_Without_Detailed_Query()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync(OrganizationType.Company);
            var dpr1 = await CreateDPRAsync(organization.Uuid);
            var dpr2 = await CreateDPRAsync(organization.Uuid);
            var dpr3 = await CreateDPRAsync(organization.Uuid);

            await DeleteDprAsync(token, dpr3);
            await DeleteDprAsync(token, dpr1);

            //Act
            var deletedItems = (await DeltaFeedV2Helper.GetDeletedEntitiesAsync(token)).ToList();

            //Assert that the right deleted items are returned in the right order (user is on company org, so no shared data should pollute the output)
            Assert.Equal(new[] { dpr3.Uuid, dpr1.Uuid }, deletedItems.Select(x => x.EntityUuid));
            Assert.All(deletedItems, x => Assert.Equal(TrackedEntityTypeChoice.DataProcessingRegistration, x.EntityType));
        }

        [Fact]
        public async Task GetDeletedItems_With_Paging()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync(OrganizationType.Company);
            var dpr1 = await CreateDPRAsync(organization.Uuid);
            var dpr2 = await CreateDPRAsync(organization.Uuid);
            var dpr3 = await CreateDPRAsync(organization.Uuid);
            var dpr4 = await CreateDPRAsync(organization.Uuid);

            await DeleteDprAsync(token, dpr3);
            await DeleteDprAsync(token, dpr1);
            await DeleteDprAsync(token, dpr2);
            await DeleteDprAsync(token, dpr4);

            //Act
            var deletedItems1 = (await DeltaFeedV2Helper.GetDeletedEntitiesAsync(token, page: 0, pageSize: 2)).ToList();
            var deletedItems2 = (await DeltaFeedV2Helper.GetDeletedEntitiesAsync(token, page: 1, pageSize: 2)).ToList();

            //Assert that the right deleted items are returned in the right order (user is on company org, so no shared data should pollute the output)
            Assert.Equal(new[] { dpr3.Uuid, dpr1.Uuid }, deletedItems1.Select(x => x.EntityUuid));
            Assert.Equal(new[] { dpr2.Uuid, dpr4.Uuid }, deletedItems2.Select(x => x.EntityUuid));
        }

        [Fact]
        public async Task GetDeletedItems_With_DeletedSinceConstraint()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync(OrganizationType.Company);
            var dpr1 = await CreateDPRAsync(organization.Uuid);
            var dpr2 = await CreateDPRAsync(organization.Uuid);
            var dpr3 = await CreateDPRAsync(organization.Uuid);
            var dpr4 = await CreateDPRAsync(organization.Uuid);

            await DeleteDprAsync(token, dpr3);
            await DeleteDprAsync(token, dpr1);
            await DeleteDprAsync(token, dpr2);
            await DeleteDprAsync(token, dpr4);
            var all = (await DeltaFeedV2Helper.GetDeletedEntitiesAsync(token)).ToList();
            var thirdItemDeletedAt = all.Skip(2).First().OccurredAtUtc;

            //Act
            var deletedItemsFiltered = (await DeltaFeedV2Helper.GetDeletedEntitiesAsync(token, deletedSinceUTC: thirdItemDeletedAt)).ToList();

            //Assert that that the last two are iuncluded (filtering includes changes AT the data provided
            Assert.Equal(new[] { dpr2.Uuid, dpr4.Uuid }, deletedItemsFiltered.Select(x => x.EntityUuid));

            //Act - add one tick - now we expect only one result
            deletedItemsFiltered = (await DeltaFeedV2Helper.GetDeletedEntitiesAsync(token, deletedSinceUTC: thirdItemDeletedAt.AddTicks(1))).ToList();

            //Assert that that the last two are iuncluded (filtering includes changes AT the data provided
            Assert.Equal(new[] { dpr4.Uuid }, deletedItemsFiltered.Select(x => x.EntityUuid));
        }

        [Theory]
        [InlineData(TrackedEntityTypeChoice.DataProcessingRegistration)]
        [InlineData(TrackedEntityTypeChoice.ItContract)]
        [InlineData(TrackedEntityTypeChoice.ItSystem)]
        [InlineData(TrackedEntityTypeChoice.ItSystemUsage)]
        [InlineData(TrackedEntityTypeChoice.ItInterface)]
        [InlineData(TrackedEntityTypeChoice.OrganizationUnit)]
        public async Task GetDeletedItems_With_EntityTypeFilter(TrackedEntityTypeChoice entityType)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync(OrganizationType.Company);
            var dpr = await CreateDPRAsync(organization.Uuid);
            var system = await CreateItSystemAsync(organization.Uuid);
            var systemUsage = await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            var contract = await CreateItContractAsync(organization.Uuid);
            var itInterface = await CreateItInterfaceAsync(organization.Uuid);
            var createdOrgUnit = await CreateOrganizationUnitAsync(organization.Uuid);

            await DeleteDprAsync(token, dpr);
            await ItContractV2Helper.DeleteContractAsync(token, contract.Uuid);
            await ItSystemV2Helper.SendDeleteSystemAsync(await GetGlobalToken(), system.Uuid);
            await ItSystemUsageV2Helper.DeleteAsync(token, systemUsage.Uuid);
            await InterfaceV2Helper.SendDeleteItInterfaceAsync(await GetGlobalToken(), itInterface.Uuid);
            await OrganizationUnitV2Helper.SendDeleteUnitAsync(organization.Uuid, createdOrgUnit.Uuid)
                .WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Act
            var dtos = await DeltaFeedV2Helper.GetDeletedEntitiesAsync(token, entityType);

            //Assert
            var singleResponse = Assert.Single(dtos);
            Assert.Equal(entityType, singleResponse.EntityType);
        }

        private static async Task DeleteDprAsync(string token, DataProcessingRegistrationResponseDTO dpr1)
        {
            await DataProcessingRegistrationV2Helper.DeleteAsync(token, dpr1.Uuid);
        }

        private async Task<(string token, User user, ShallowOrganizationResponseDTO organization)> CreatePrerequisitesAsync(OrganizationType organizationType)
        {
            var organization = await CreateOrganizationAsync(type: organizationType);
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            return (token, user, organization);
        }

        private async Task<(User user, string token)> CreateApiUser(ShallowOrganizationResponseDTO organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Uuid, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ByUuid(userAndGetToken.userUuid));
            return (user, userAndGetToken.token);
        }
    }
}
