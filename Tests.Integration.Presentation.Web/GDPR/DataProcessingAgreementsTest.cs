using System.Net;
using System.Threading.Tasks;
using ExpectedObjects;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.GDPR
{
    public class DataProcessingAgreementsTest : WithAutoFixture
    {
        [Fact]
        public async Task Can_Create()
        {
            //Arrange
            var name = A<string>();

            //Act
            var response = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);

            //Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Can_Create_With_Same_Name_In_Different_Organizations()
        {
            //Arrange
            var name = A<string>();

            //Act
            var responseInFirstOrg = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);
            var responseInSecondOrg = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.SecondOrganizationId, name).ConfigureAwait(false);

            //Assert
            Assert.NotNull(responseInFirstOrg);
            Assert.NotNull(responseInSecondOrg);
            Assert.NotEqual(responseInFirstOrg.Id, responseInSecondOrg.Id);
            Assert.Equal(responseInFirstOrg.Name, responseInSecondOrg.Name);
        }

        [Fact]
        public async Task Cannot_Create_With_Duplicate_Name_In_Same_Organization()
        {
            //Arrange
            var name = A<string>();
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            //Act
            await DataProcessingAgreementHelper.CreateAsync(organizationId, name).ConfigureAwait(false);
            using (var secondResponse = await DataProcessingAgreementHelper.SendCreateRequestAsync(organizationId, name).ConfigureAwait(false))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Can_Get()
        {
            //Arrange
            var name = A<string>();
            var dto = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);

            //Act
            var gotten = await DataProcessingAgreementHelper.GetAsync(dto.Id);

            //Assert
            Assert.NotNull(gotten);
            dto.ToExpectedObject().ShouldMatch(gotten);
        }

        [Fact]
        public async Task Can_Delete()
        {
            //Arrange
            var name = A<string>();
            var dto = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name)
                .ConfigureAwait(false);

            //Act
            using (var deleteResponse = await DataProcessingAgreementHelper.SendDeleteRequestAsync(dto.Id))
            {
                //Assert
                Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Can_Change_Name()
        {
            //Arrange
            var name1 = A<string>();
            var name2 = A<string>();
            var agreementDto = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name1).ConfigureAwait(false);

            //Act
            using (var response = await DataProcessingAgreementHelper.SendChangeNameRequestAsync(agreementDto.Id, name2))
            {
                //Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task Cannot_Change_Name_To_NonUniqueName_In_Same_Org()
        {
            //Arrange
            var name1 = A<string>();
            var name2 = A<string>();
            await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name1).ConfigureAwait(false);
            var agreementDto = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name2).ConfigureAwait(false);

            //Act
            using (var response = await DataProcessingAgreementHelper.SendChangeNameRequestAsync(agreementDto.Id, name1))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            }
        }

        [Fact]
        public async Task Can_Create_Endpoint_Returns_Ok()
        {
            //Arrange
            var name = A<string>();

            //Act
            using (var response = await DataProcessingAgreementHelper.SendCanCreateRequestAsync(TestEnvironment.DefaultOrganizationId, name))
            {
                //Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task Can_Create_Endpoint_Returns_Error_If_Non_Unique()
        {
            //Arrange
            var name = A<string>();
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            await DataProcessingAgreementHelper.CreateAsync(organizationId, name).ConfigureAwait(false);

            //Act
            using (var response = await DataProcessingAgreementHelper.SendCanCreateRequestAsync(organizationId, name))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            }
        }

        [Theory]
        [InlineData("")] //too short
        [InlineData("    ")] //only whitespace
        [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901")] //101 chars
        public async Task Can_Create_Returns_Error_If_InvalidName(string name)
        {
            //Act
            using (var response = await DataProcessingAgreementHelper.SendCanCreateRequestAsync(TestEnvironment.DefaultOrganizationId, name))
            {
                //Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        //TODO: Paging query
        //TODO: Read model test! + query
    }
}
