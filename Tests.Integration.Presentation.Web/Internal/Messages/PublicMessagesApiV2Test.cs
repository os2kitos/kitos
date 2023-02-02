using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Response;
using Presentation.Web.Models.API.V2.Response.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Internal.Messages
{
    public class PublicMessagesApiV2Test : WithAutoFixture
    {
        private readonly Uri _rootUrl = TestEnvironment.CreateUrl("api/v2/internal/public-messages");

        [Fact]
        public async Task Can_GET()
        {
            //Arrange
            var texts = ChangePublicMessages();

            //Act
            using var response = await HttpApi.GetAsync(_rootUrl);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var messages = await response.ReadResponseBodyAsAsync<PublicMessagesResponseDTO>();
            Assert.Equivalent(texts, messages);
        }

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin, false)]
        [InlineData(OrganizationRole.User, false)]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        public async Task Can_GET_Permissions(OrganizationRole role, bool allowModify)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);
            var expected = ChangePublicMessages();
            var newText = A<string>();
            expected.About = newText;

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
            var expected = ChangePublicMessages();
            var newText = A<string>();
            expected.About = newText;

            //Act
            using var patchResponse = await HttpApi.PatchWithCookieAsync(_rootUrl, cookie, new
            {
                About = newText
            });

            //Assert that only changed property was actually changed
            Assert.Equal(HttpStatusCode.Forbidden, patchResponse.StatusCode);
        }

        [Fact]
        public async Task Can_PATCH_About()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var expected = ChangePublicMessages();
            var newText = A<string>();
            expected.About = newText;

            //Act
            using var patchResponse = await HttpApi.PatchWithCookieAsync(_rootUrl, cookie, new
            {
                About = newText
            });

            //Assert that only changed property was actually changed
            await AssertPatchSucceeded(patchResponse, expected);
        }

        [Fact]
        public async Task Can_PATCH_Guides()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var expected = ChangePublicMessages();
            var newText = A<string>();
            expected.Guides = newText;

            //Act
            using var patchResponse = await HttpApi.PatchWithCookieAsync(_rootUrl, cookie, new
            {
                Guides = newText
            });

            //Assert that only changed property was actually changed
            await AssertPatchSucceeded(patchResponse, expected);
        }

        [Fact]
        public async Task Can_PATCH_ContactInfo()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var expected = ChangePublicMessages();
            var newText = A<string>();
            expected.ContactInfo = newText;

            //Act
            using var patchResponse = await HttpApi.PatchWithCookieAsync(_rootUrl, cookie, new
            {
                ContactInfo = newText
            });

            //Assert that only changed property was actually changed
            await AssertPatchSucceeded(patchResponse, expected);
        }

        [Fact]
        public async Task Can_PATCH_Misc()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var expected = ChangePublicMessages();
            var newText = A<string>();
            expected.Misc = newText;

            //Act
            using var patchResponse = await HttpApi.PatchWithCookieAsync(_rootUrl, cookie, new
            {
                Misc = newText
            });

            //Assert that only changed property was actually changed
            await AssertPatchSucceeded(patchResponse, expected);
        }

        [Fact]
        public async Task Can_PATCH_StatusMessages()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var expected = ChangePublicMessages();
            var newText = A<string>();
            expected.StatusMessages = newText;

            //Act
            using var patchResponse = await HttpApi.PatchWithCookieAsync(_rootUrl, cookie, new
            {
                StatusMessages = newText
            });

            //Assert that only changed property was actually changed
            await AssertPatchSucceeded(patchResponse, expected);
        }

        [Fact]
        public async Task Can_PATCH_ALL()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var expected = ChangePublicMessages();
            expected.StatusMessages = A<string>();
            expected.About = A<string>();
            expected.Guides = A<string>();
            expected.Misc = A<string>();
            expected.ContactInfo = A<string>();

            //Act
            using var patchResponse = await HttpApi.PatchWithCookieAsync(_rootUrl, cookie, new
            {
                expected.StatusMessages,
                expected.About,
                expected.ContactInfo,
                expected.Misc,
                expected.Guides
            });

            //Assert that only changed property was actually changed
            await AssertPatchSucceeded(patchResponse, expected);
        }

        private static PublicMessagesResponseDTO ChangePublicMessages()
        {
            var expectedResponse = new PublicMessagesResponseDTO();
            DatabaseAccess.MutateEntitySet<Text>(textsRepo =>
            {
                var texts = textsRepo.AsQueryable().ToDictionary(x => x.Id);
                foreach (var text in texts)
                {
                    text.Value.Value = Guid.NewGuid().ToString();
                }

                expectedResponse = new PublicMessagesResponseDTO
                {
                    About = texts[Text.SectionIds.About].Value,
                    ContactInfo = texts[Text.SectionIds.ContactInfo].Value,
                    Guides = texts[Text.SectionIds.Guides].Value,
                    Misc = texts[Text.SectionIds.Misc].Value,
                    StatusMessages = texts[Text.SectionIds.StatusMessages].Value
                };
            });
            return expectedResponse;
        }

        private static async Task AssertPatchSucceeded(HttpResponseMessage patchResponse, PublicMessagesResponseDTO expected)
        {
            Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
            var changedMessages = await patchResponse.ReadResponseBodyAsAsync<PublicMessagesResponseDTO>();
            Assert.Equivalent(expected, changedMessages);
        }
    }
}
