using System.Net;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Xunit;

namespace Tests.Integration.Presentation.Web.Swagger
{
    [Collection(nameof(SequentialTestGroup))]
    public class SwaggerDocumentationTest
    {
        public class SwaggerDoc
        {
            public string Swagger { get; set; }
            public string Host { get; set; }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task Can_Load_Swagger_Doc(int version)
        {
            //Arrange
            var url = TestEnvironment.CreateUrl($"/swagger/docs/{version}");

            //Act
            using var result = await HttpApi.GetAsync(url);

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var doc = await result.ReadResponseBodyAsAsync<SwaggerDoc>();
            Assert.Equal("2.0", doc.Swagger);
            Assert.Equal(url.Authority, doc.Host);
        }
    }
}
