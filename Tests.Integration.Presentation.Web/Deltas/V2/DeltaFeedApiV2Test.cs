using System;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Contract;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Deltas.V2
{
    public class DeltaFeedApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task GetDeletedItems_Without_Detailed_Query()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync(OrganizationTypeKeys.Virksomhed);
            var dpr1 = await CreateDprAsync(token, organization);
            var dpr2 = await CreateDprAsync(token, organization);
            var dpr3 = await CreateDprAsync(token, organization);

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
            var (token, user, organization) = await CreatePrerequisitesAsync(OrganizationTypeKeys.Virksomhed);
            var dpr1 = await CreateDprAsync(token, organization);
            var dpr2 = await CreateDprAsync(token, organization);
            var dpr3 = await CreateDprAsync(token, organization);
            var dpr4 = await CreateDprAsync(token, organization);

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
            var (token, user, organization) = await CreatePrerequisitesAsync(OrganizationTypeKeys.Virksomhed);
            var dpr1 = await CreateDprAsync(token, organization);
            var dpr2 = await CreateDprAsync(token, organization);
            var dpr3 = await CreateDprAsync(token, organization);
            var dpr4 = await CreateDprAsync(token, organization);

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
        [InlineData(TrackedEntityTypeChoice.ItProject)]
        [InlineData(TrackedEntityTypeChoice.ItSystem)]
        [InlineData(TrackedEntityTypeChoice.ItSystemUsage)]
        [InlineData(TrackedEntityTypeChoice.ItInterface)]
        public async Task GetDeletedItems_With_EntityTypeFilter(TrackedEntityTypeChoice entityType)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync(OrganizationTypeKeys.Virksomhed);
            var dpr = await CreateDprAsync(token, organization);
            var system = await CreateItItSystemAsync(organization);
            var systemUsage = await CreateItItSystemUsageAsync(token, organization);
            var project = await CreateItProjectAsync(organization);
            var contract = await CreateItContractAsync(token, organization);
            var itInterface = await CreateItItInterfaceAsync(token, organization);

            await DeleteDprAsync(token, dpr);
            await ItContractV2Helper.DeleteContractAsync(token, contract.Uuid);
            await ItProjectHelper.DeleteProjectAsync(project.Id);
            await ItSystemHelper.DeleteItSystemAsync(system.Id, system.OrganizationId);
            await ItSystemUsageV2Helper.DeleteAsync(token, systemUsage.Uuid);
            await InterfaceHelper.DeleteInterfaceAsync(itInterface.Id);

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

        private async Task<DataProcessingRegistrationResponseDTO> CreateDprAsync(string token, OrganizationDTO organization)
        {
            return await DataProcessingRegistrationV2Helper.PostAsync(token, new CreateDataProcessingRegistrationRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName()
            });
        }

        private async Task<ItProjectDTO> CreateItProjectAsync(OrganizationDTO organization)
        {
            return await ItProjectHelper.CreateProject(CreateName(), organization.Id);
        }

        private async Task<ItContractResponseDTO> CreateItContractAsync(string token, OrganizationDTO organization)
        {
            return await ItContractV2Helper.PostContractAsync(token, new CreateNewContractRequestDTO() { OrganizationUuid = organization.Uuid, Name = CreateName() });
        }

        private async Task<ItSystemDTO> CreateItItSystemAsync(OrganizationDTO organization)
        {
            return await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
        }

        private async Task<ItSystemUsageResponseDTO> CreateItItSystemUsageAsync(string token, OrganizationDTO organization)
        {
            var itItSystemAsync = await CreateItItSystemAsync(organization);
            return await ItSystemUsageV2Helper.PostAsync(token, new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = itItSystemAsync.Uuid });
        }

        private async Task<ItInterfaceDTO> CreateItItInterfaceAsync(string token, OrganizationDTO organization)
        {
            return await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(CreateName(), A<string>(), organization.Id, AccessModifier.Public));
        }

        private async Task<(string token, User user, OrganizationDTO organization)> CreatePrerequisitesAsync(OrganizationTypeKeys organizationType)
        {
            var organization = await CreateOrganizationAsync(organizationType);
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            return (token, user, organization);
        }

        private async Task<OrganizationDTO> CreateOrganizationAsync(OrganizationTypeKeys orgType)
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, string.Empty, orgType, AccessModifier.Public);
            return organization;
        }

        private async Task<(User user, string token)> CreateApiUser(OrganizationDTO organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }

        private string CreateName()
        {
            return $"{nameof(DeltaFeedApiV2Test)}{A<string>()}";
        }

        private string CreateEmail()
        {
            return $"{CreateName()}{DateTime.Now.Ticks}@kitos.dk";
        }
    }
}
