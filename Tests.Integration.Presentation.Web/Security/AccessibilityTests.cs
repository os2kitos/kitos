using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;
using Tests.Integration.Presentation.Web.Tools.Model;

namespace Tests.Integration.Presentation.Web.Security
{
    public class AccessibilityTests : WithAutoFixture
    {
        private readonly string _defaultPassword;

        public AccessibilityTests()
        {
            _defaultPassword = TestEnvironment.GetDefaultUserPassword();
        }

        [Theory]
        [InlineData("api/User", HttpStatusCode.Forbidden)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Forbidden)]
        [InlineData("odata/ItSystems", HttpStatusCode.OK)]
        public async Task Api_Get_Requests_Using_Token(string apiUrl, HttpStatusCode httpCode)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(apiUrl), token.Token))
            {
                //Assert
                Assert.Equal(httpCode, httpResponse.StatusCode);
            }
        }

        [Theory]
        [InlineData("api/User", HttpStatusCode.Unauthorized)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Unauthorized)]
        [InlineData("odata/ItSystems", HttpStatusCode.Unauthorized)]
        public async Task Anonymous_Api_Calls_Returns_401(string apiUrl, HttpStatusCode httpCode)
        {
            using (var httpResponse = await HttpApi.GetAsync(TestEnvironment.CreateUrl(apiUrl)))
            {
                Assert.Equal(httpCode, httpResponse.StatusCode);
            }
        }

        [Theory]
        [InlineData("odata/itsystems", typeof(Core.DomainModel.ItSystem.ItSystem))]
        [InlineData("api/itsystem", typeof(Core.DomainModel.ItSystem.ItSystem))]
        [InlineData("odata/itinterfaces", typeof(ItInterface))]
        [InlineData("api/itinterface", typeof(ItInterface))]
        [InlineData("odata/reports", typeof(Report))]
        [InlineData("api/report", typeof(Report))]
        [InlineData("odata/itsystemusages", typeof(ItSystemUsage))]
        [InlineData("api/itsystemusage", typeof(ItSystemUsage))]
        [InlineData("odata/itcontracts", typeof(ItContract))]
        [InlineData("api/itcontract", typeof(ItContract))]
        [InlineData("odata/itprojects", typeof(ItProject))]
        [InlineData("api/itproject", typeof(ItProject))]
        public async Task Api_Is_Read_Only(string path, Type inputType)
        {
            //Arrange
            var globalAdminToken = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            //Act
            using (var httpResponse = await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl(path), Activator.CreateInstance(inputType), globalAdminToken.Token))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, httpResponse.StatusCode);
                var message = await httpResponse.Content.ReadAsStringAsync();
                Assert.Equal("Det er ikke tilladt at skrive data via APIet", message);
            }
        }

        [Fact]
        public async Task Post_Reference_With_Valid_Input_Returns_201()
        {
            //Arrange
            var contract = await ItContractHelper.CreateContract(A<string>(), TestEnvironment.DefaultOrganizationId);
            var payload = new
            {
                Title = A<string>(),
                ExternalReferenceId = A<string>(),
                URL = "https://strongminds.dk/",
                Itcontract_Id = contract.Id
            };
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);

            //Act
            using (var httpResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl("/api/Reference"), cookie, payload))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Token_Can_Be_Invalidated_After_Creation()
        {
            //Arrange
            var email = CreateEmail();
            var userDto = ObjectCreateHelper.MakeSimpleApiUserDto(email, true);
            var createdUserId = await HttpApi.CreateOdataUserAsync(userDto, OrganizationRole.User);
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(email, _defaultPassword);
            var token = await HttpApi.GetTokenAsync(loginDto);
            using (var requestResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("api/ItSystem/"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.OK, requestResponse.StatusCode);
            };

            //Act
            await DisableApiAccessForUserAsync(userDto, createdUserId);

            //Assert
            using (var requestResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("api/ItSystem/"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.Forbidden, requestResponse.StatusCode);
            };
            await HttpApi.DeleteUserAsync(createdUserId);
        }

        private static string CreateEmail()
        {
            return $"{Guid.NewGuid():N}@test.dk";
        }

        private static async Task DisableApiAccessForUserAsync(ApiUserDTO userDto, int id)
        {
            userDto.HasApiAccess = false;
            await HttpApi.PatchOdataUserAsync(userDto, id);
        }

    }
}
