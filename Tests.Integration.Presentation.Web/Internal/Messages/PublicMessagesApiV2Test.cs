using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.Internal.Messages
{
    public class PublicMessagesApiV2Test
    {

        [Fact]
        public async Task Can_GET()
        {
            //Arrange
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

            //Act
            using var response = await HttpApi.GetAsync(TestEnvironment.CreateUrl("api/v2/internal/public-messages"));

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var messages = await response.ReadResponseBodyAsAsync<PublicMessagesResponseDTO>();
            Assert.Equivalent(expectedResponse, messages);
        }
    }
}
