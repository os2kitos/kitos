using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.LocalAdminArea
{
    public class ExcelImportTest : WithAutoFixture
    {

        private readonly string excelApiPrefix = "api/local-admin/excel";
        private readonly string byIdApiPrefix = "contracts-by-id";
        private readonly string byUuidApiPrefix = "contracts-by-uuid";

        [Fact]
        public async Task Can_Import_Contracts_From_Excel_By_Id()
        {
            //Arrange
            var (organizationDto, loginCookie, content) = await SetupSuccessfulPostExcel();

            //Act
            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{excelApiPrefix}/{byIdApiPrefix}?organizationId={organizationDto.Id}"), loginCookie, content);

            //Assert
            AssertSuccessfulPostExcel(response, organizationDto.Id);
        }

        [Fact]
        public async Task Can_Import_Contracts_From_Excel_By_Uuid()
        {
            //Arrange
            var (organizationDto, loginCookie, content) = await SetupSuccessfulPostExcel();

            //Act
            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{excelApiPrefix}/{byUuidApiPrefix}?organizationUuid={organizationDto.Uuid}"), loginCookie, content);

            //Assert
            AssertSuccessfulPostExcel(response, organizationDto.Id);
        }

        private async Task<(OrganizationDTO, Cookie, MultipartFormDataContent)> SetupSuccessfulPostExcel()
        {
            var organizationDto = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, A<string>(), "", OrganizationTypeKeys.AndenOffentligMyndighed, AccessModifier.Public);
            var (_, _, loginCookie) = await HttpApi.CreateUserAndLogin($"email{A<Guid>():N}@test.dk", OrganizationRole.LocalAdmin, organizationDto.Id);
            var content = new MultipartFormDataContent
            {
                new ByteArrayContent(Resources.valid_it_contract_example)
            };
            return (organizationDto, loginCookie, content);
        }

        private void AssertSuccessfulPostExcel(HttpResponseMessage response, int organizationId)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var expectedNames = new[] { "test-kommune testkontrakt1", "test-kommune testkontrakt2" };
            foreach (var expectedName in expectedNames)
            {
                Assert.True(DatabaseAccess.MapFromEntitySet<ItContract, bool>(c => c.AsQueryable().ByOrganizationId(organizationId).ByNameExact(expectedName).Count() == 1));
                Assert.True(DatabaseAccess.MapFromEntitySet<ItContractOverviewReadModel, bool>(c => c.AsQueryable().ByOrganizationId(organizationId).ByNameExact(expectedName).Count() == 1));
            }
        }

        [Fact]
        public async Task Cannot_Import_Invalid_Contracts_From_Excel_By_Id()
        {
            //Arrange
            var organizationDto = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, A<string>(), "", OrganizationTypeKeys.AndenOffentligMyndighed, AccessModifier.Public);
            var (_, _, loginCookie) = await HttpApi.CreateUserAndLogin($"email{A<Guid>():N}@test.dk", OrganizationRole.LocalAdmin, organizationDto.Id);
            var content = new MultipartFormDataContent
            {
                new ByteArrayContent(Resources.invalid_contract_example)
            };

            //Act
            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{excelApiPrefix}/{byIdApiPrefix}?organizationId={organizationDto.Id}"), loginCookie, content);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Import_Invalid_Contracts_From_Excel_By_Uuid()
        {
            //Arrange
            var organizationDto = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, A<string>(), "", OrganizationTypeKeys.AndenOffentligMyndighed, AccessModifier.Public);
            var (_, _, loginCookie) = await HttpApi.CreateUserAndLogin($"email{A<Guid>():N}@test.dk", OrganizationRole.LocalAdmin, organizationDto.Id);
            var content = new MultipartFormDataContent
            {
                new ByteArrayContent(Resources.invalid_contract_example)
            };

            //Act
            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{excelApiPrefix}/{byIdApiPrefix}?organizationId={organizationDto.Id}"), loginCookie, content);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
    }
}
