using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;

namespace Tests.Integration.Presentation.Web.LocalAdminArea
{
    public class ExcelImportTest : BaseTest
    {
        private const string ExcelApiPrefix = "api/local-admin/excel";
        private const string ByUuidApiPrefix = "contracts-by-uuid";

        [Fact]
        public async Task Can_Import_Contracts_From_Excel_By_Uuid()
        {
            //Arrange
            var (organizationDto, loginCookie, content) = await SetupSuccessfulPostExcel();

            //Act
            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{ExcelApiPrefix}/{ByUuidApiPrefix}?organizationUuid={organizationDto.Uuid}"), loginCookie, content);

            //Assert
            AssertSuccessfulPostExcel(response, organizationDto.Uuid);
        }

        private async Task<(ShallowOrganizationResponseDTO, Cookie, MultipartFormDataContent)> SetupSuccessfulPostExcel()
        {
            var organizationDto = await CreateOrganizationAsync(type: OrganizationType.OtherPublicAuthority);
            var (_, _, loginCookie) = await HttpApi.CreateUserAndLogin($"email{A<Guid>():N}@test.dk", OrganizationRole.LocalAdmin, organizationDto.Uuid);
            var content = new MultipartFormDataContent
            {
                new ByteArrayContent(Resources.valid_it_contract_example)
            };
            return (organizationDto, loginCookie, content);
        }

        private void AssertSuccessfulPostExcel(HttpResponseMessage response, Guid organizationUuid)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var expectedNames = new[] { "test-kommune testkontrakt1", "test-kommune testkontrakt2" };
            foreach (var expectedName in expectedNames)
            {
                Assert.True(DatabaseAccess.MapFromEntitySet<ItContract, bool>(c => c.AsQueryable().ByOrganizationUuid(organizationUuid).ByNameExact(expectedName).Count() == 1));
                Assert.True(DatabaseAccess.MapFromEntitySet<ItContractOverviewReadModel, bool>(c => c.AsQueryable().ByOrganizationUuid(organizationUuid).ByNameExact(expectedName).Count() == 1));
            }
        }

        [Fact]
        public async Task Cannot_Import_Invalid_Contracts_From_Excel_By_Uuid()
        {
            //Arrange
            var organizationDto = await CreateOrganizationAsync(type: OrganizationType.OtherPublicAuthority);
            var (_, _, loginCookie) = await HttpApi.CreateUserAndLogin($"email{A<Guid>():N}@test.dk", OrganizationRole.LocalAdmin, organizationDto.Uuid);
            var content = new MultipartFormDataContent
            {
                new ByteArrayContent(Resources.invalid_contract_example)
            };

            //Act
            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{ExcelApiPrefix}/{ByUuidApiPrefix}?organizationUuid={organizationDto.Uuid}"), loginCookie, content);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
    }
}
