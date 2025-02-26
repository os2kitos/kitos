using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Core.DomainModel.PublicMessage;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Response;
using Presentation.Web.Models.API.V2.Response.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Internal.Messages
{
    public class PublicMessagesApiV2Test : WithAutoFixture
    {
        private const string BasePath = "api/v2/internal/public-messages";
        private readonly Uri _rootUrl = TestEnvironment.CreateUrl(BasePath);

        [Fact]
        public async Task Can_GET()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var createdMessage = await SetupPublicMessage(cookie);

            //Act
            var response = await GetPublicMessages();

            //Assert
            Assert.Contains(response,
                x => x.Uuid == createdMessage.Uuid &&
                     x.LongDescription == createdMessage.LongDescription &&
                     x.Link == createdMessage.Link &&
                     x.ShortDescription == createdMessage.ShortDescription &&
                     x.Status == createdMessage.Status);
        }

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin, false)]
        [InlineData(OrganizationRole.User, false)]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        public async Task Can_GET_Permissions(OrganizationRole role, bool allowModify)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);

            //Act
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl("api/v2/internal/public-messages/permissions"), cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var permissions = await response.ReadResponseBodyAsAsync<ResourcePermissionsResponseDTO>();
            Assert.True(permissions.Read);      // All users can read
            Assert.False(permissions.Delete);   // No one can delete
            Assert.Equal(allowModify, permissions.Modify);
        }

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_PATCH_If_Not_Global_Admin(OrganizationRole role)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);
            var publicMessage = await SetupPublicMessage();

            //Act
            using var patchResponse = await HttpApi.PatchWithCookieAsync(GetUrlWithUuid(publicMessage.Uuid), cookie, new
            {
                LongDescription = A<string>()
            });

            //Assert that only changed property was actually changed
            Assert.Equal(HttpStatusCode.Forbidden, patchResponse.StatusCode);
        }

        [Fact]
        public async Task Can_PATCH()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var publicMessage = await SetupPublicMessage(cookie);

            var patchRequest = new PublicMessageRequestDTO
            {
                Link = A<string>(),
                Title = A<string>(),
                LongDescription = A<string>(),
                ShortDescription = A<string>(),
                Status = A<PublicMessageStatusChoice>()
            };

            //Act
            using var patchResponse = await HttpApi.PatchWithCookieAsync(GetUrlWithUuid(publicMessage.Uuid), cookie, patchRequest);

            //Assert that only changed property was actually changed
            await AssertPatchSucceeded(patchResponse, patchRequest);
        }
        
        private async Task<PublicMessageResponseDTO> SetupPublicMessage(Cookie cookie = null)
        {

            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var publicMessages = DatabaseAccess.MapFromEntitySet<PublicMessage, List<PublicMessage>>(x => x.AsQueryable().ToList());
            if (publicMessages.Any() == false)
            {
                var request = A<PublicMessageRequestDTO>();
                using var postResponse = await HttpApi.PostWithCookieAsync(_rootUrl, requestCookie, request);

                Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
                return await postResponse.ReadResponseBodyAsAsync<PublicMessageResponseDTO>();
            }

            var publicMessage = publicMessages.First();
            return new PublicMessageResponseDTO(publicMessage);
        }

        private async Task<IEnumerable<PublicMessageResponseDTO>> GetPublicMessages()
        {
            using var response = await HttpApi.GetAsync(_rootUrl);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<List<PublicMessageResponseDTO>>();
        }

        private static async Task AssertPatchSucceeded(HttpResponseMessage patchResponse, PublicMessageRequestDTO expected)
        {
            Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
            var changedMessages = await patchResponse.ReadResponseBodyAsAsync<PublicMessageResponseDTO>();
            Assert.Equivalent(expected, changedMessages);
        }

        private static Uri GetUrlWithUuid(Guid uuid)
        {
            return TestEnvironment.CreateUrl($"{BasePath}/{uuid}");
        }
    }
}
