using System;
using System.Net;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Request.Contract;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Xunit;

namespace Tests.Integration.Presentation.Web.Swagger
{
    [Collection(nameof(SequentialTestGroup))]
    public class SwaggerDocumentationTest
    {
        private class SwaggerDoc
        {
            public required string OpenApi { get; set; }
            public required SwaggerInfo Info { get; set; }
        }

        private class SwaggerInfo
        {
            public required int Version { get; set; }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task Can_Load_Swagger_Doc(int version)
        {
            //Arrange
            var url = CreateUrl(version);

            //Act
            using var result = await HttpApi.GetAsync(url);

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var res = await result.Content.ReadAsStringAsync();
            var doc = await result.ReadResponseBodyAsAsync<SwaggerDoc>();
            Assert.Equal("3.0.4", doc.OpenApi);
            Assert.Equal(version, doc.Info.Version);
        }

        [Fact]
        public async Task Swagger_Doc_V1_Contains_Correct_Types()
        {
            //Arrange
            var url = CreateUrl(1);

            //Act
            using var result = await HttpApi.GetAsync(url);

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var body = await result.Content.ReadAsStringAsync();
            Assert.DoesNotContain(nameof(ContractGeneralDataWriteRequestDTO), body);
            Assert.Contains(nameof(GetTokenResponseDTO), body);
        }

        [Fact]
        public async Task Swagger_Doc_V2_Contains_Correct_Types()
        {
            //Arrange
            var url = CreateUrl(2);

            //Act
            using var result = await HttpApi.GetAsync(url);

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var body = await result.Content.ReadAsStringAsync();
            Assert.Contains(nameof(ContractGeneralDataWriteRequestDTO), body);
            Assert.DoesNotContain(nameof(GetTokenResponseDTO), body);
        }

        private static Uri CreateUrl(int apiVersion)
        {
            return TestEnvironment.CreateUrl($"/swagger/v{apiVersion}/swagger.json");
        }
    }
}
