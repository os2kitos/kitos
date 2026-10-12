using System;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.System;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Xunit;

namespace Tests.Integration.Presentation.Web.SystemUsage.V2
{
    public class ItSystemUsageArchiveV2ApiTest : BaseItSystemUsageApiV2Test
    {
        [Fact]
        public async Task Can_Get_Archive_By_Uuid()
        {
            // Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var systemUsage = await TakeSystemIntoUsageAsync(system.Uuid, organization.Uuid);
            
            var archive = await ItSystemUsageV2Helper.ArchiveAsync(token, systemUsage.Uuid, CreateArchiveRequest());

            // Act
            var result = await ItSystemUsageArchiveV2Helper.GetArchiveAsync(token, archive.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(archive.Uuid, result.Uuid);
            Assert.Equal(archive.ReferenceName, result.ReferenceName);
            Assert.Equal(user.GetFullName(), result.ArchivedByFullName);
        }

        [Fact]
        public async Task Can_Delete_Archive()
        {
            // Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var systemUsage = await TakeSystemIntoUsageAsync(system.Uuid, organization.Uuid);
            
            var archive = await ItSystemUsageV2Helper.ArchiveAsync(token, systemUsage.Uuid, CreateArchiveRequest());

            // Act
            await ItSystemUsageArchiveV2Helper.DeleteArchiveAsync(token, archive.Uuid);

            // Assert - Verify archive is deleted by attempting to get it
            var getResult = await ItSystemUsageArchiveV2Helper.SendGetArchiveAsync(token, archive.Uuid);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResult.StatusCode);
        }

        private CreateItSystemUsageArchiveRequestDTO CreateArchiveRequest()
        {
            return new CreateItSystemUsageArchiveRequestDTO
            {
                ReferenceName = A<string>(),
                ArchivingDate = A<DateTime>(),
                TakenIntoUsageDate = A<DateTime>(),
                Note = A<string>()
            };
        }

        private async Task<(string token, User user, ShallowOrganizationResponseDTO organization, ItSystemResponseDTO system)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUser(organization.Uuid);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            var system = await CreateItSystemAsync(organization.Uuid);
            return (token, user, organization, system);
        }

        private async Task<(User user, string token)> CreateApiUser(Guid organizationUuid)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organizationUuid, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ByUuid(userAndGetToken.userUuid)!);
            return (user, userAndGetToken.token);
        }
    }
}
