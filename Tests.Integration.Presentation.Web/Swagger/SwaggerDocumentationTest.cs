using System.Net;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.Swagger
{
    public class SwaggerDocumentationTest
    {
        public class SwaggerDoc
        {
            public string Swagger { get; set; }
            public string Host { get; set; }
        }

        [Fact]
        public async Task Can_Load_Swagger_Doc()
        {
            //Arrange
            var url = TestEnvironment.CreateUrl("/swagger/docs/1.0.0");

            //Act
            using (var result = await HttpApi.GetAsync(url))
            {
                //Assert
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                var doc = await result.ReadResponseBodyAsAsync<SwaggerDoc>();
                Assert.Equal("2.0", doc.Swagger);
                Assert.Equal(url.Host, doc.Host);
            }
        }
    }
}
