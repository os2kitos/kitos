using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Response;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.HelpTexts
{
    public class HelpTextsInternalApiV2Test: WithAutoFixture
    {
        [Fact]
        public async Task Can_Get_HelpText()
        {
            var expected = A<HelpTextCreateRequestDTO>();
            var createResponse = await HelpTextsInternalV2Helper.Create(expected);
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

            var response = await HelpTextsInternalV2Helper.GetSingle(expected.Key);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var actual = await response.ReadResponseBodyAsAsync<HelpTextResponseDTO>();
            Assert.Equal(expected.Key, actual.Key);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.Title, actual.Title);
        }

        [Fact]
        public async Task Can_Get_HelpTexts()
        {
            var expected = A<HelpTextCreateRequestDTO>();
            var createResponse = await HelpTextsInternalV2Helper.Create(expected);
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

            var response = await HelpTextsInternalV2Helper.GetAll();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.ReadResponseBodyAsAsync<IEnumerable<HelpTextResponseDTO>>();
            var actual = content.First(ht => ht.Key == expected.Key);
            Assert.NotNull(actual);
            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public async Task Can_Create_HelpText()
        {
            var dto = A<HelpTextCreateRequestDTO>();

            var response = await HelpTextsInternalV2Helper.Create(dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var actual = await response.ReadResponseBodyAsAsync<HelpTextResponseDTO>();
            Assert.NotNull(actual);
            Assert.Equivalent(dto, actual);
        }

        [Fact]
        public async Task Can_Delete_HelpText()
        {
            var dto = A<HelpTextCreateRequestDTO>();
            var createResponse = await HelpTextsInternalV2Helper.Create(dto);
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

            var response = await HelpTextsInternalV2Helper.Delete(dto.Key);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var allHelpTextsResponse = await HelpTextsInternalV2Helper.GetAll();
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            var allHelpTexts = await allHelpTextsResponse.ReadResponseBodyAsAsync<IEnumerable<HelpText>>();
            Assert.DoesNotContain(allHelpTexts, ht => ht.Key == dto.Key);
        }

        [Fact]
        public async Task Can_Patch_HelpText()
        {
            var createRequestDto = A<HelpTextCreateRequestDTO>();
            var createResponse = await HelpTextsInternalV2Helper.Create(createRequestDto);
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            var updateDto = A<HelpTextUpdateRequestDTO>();

            var response = await HelpTextsInternalV2Helper.Patch(createRequestDto.Key, updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var allHelpTextsResponse = await HelpTextsInternalV2Helper.GetAll();
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            var allHelpTexts = await allHelpTextsResponse.ReadResponseBodyAsAsync<IEnumerable<HelpText>>();
            var updated = allHelpTexts.First(ht => ht.Key == createRequestDto.Key);
            Assert.NotNull(updated);
            Assert.Equal(updateDto.Description, updated.Description);
            Assert.Equal(updateDto.Title, updated.Title);

        }
    }
}
