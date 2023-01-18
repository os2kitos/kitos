using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;
using Presentation.Web.Controllers.API.V2.Mapping;

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
                var texts = textsRepo.AsQueryable().ToList();
                foreach (var text in texts)
                {
                    text.Value = Guid.NewGuid().ToString();
                }

                expectedResponse = texts.ToTDO();
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
