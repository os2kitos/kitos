using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItInterfaceUsageTests : WithAutoFixture
    {

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Api_User_Can_Get_It_Interface_Usage(OrganizationRole role)
        {
            //Arrange
            var dto = InterfaceHelper.CreateInterfaceDto(A<Guid>().ToString("N"), A<Guid>().ToString("N"), TestEnvironment.DefaultUserId, TestEnvironment.DefaultOrganizationId, AccessModifier.Local);

            var createdInterface = await InterfaceHelper.CreateInterface(dto);
            await InterfaceHelper.CreateItInterfaceUsageAsync(
                TestEnvironment.DefaultItSystemUsageId,
                createdInterface.Id,
                TestEnvironment.DefaultItSystemId,
                dto.OrganizationId,
                TestEnvironment.DefaultContractId);

            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"api/ItInterfaceUsage?usageId={TestEnvironment.DefaultItSystemUsageId}&sysId={TestEnvironment.DefaultItSystemId}&interfaceId={createdInterface.Id}");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<ItInterfaceUsageDTO>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.Equal(TestEnvironment.DefaultItSystemUsageId, response.ItSystemUsageId);
                Assert.Equal(TestEnvironment.DefaultItSystemId, response.ItSystemId);
                Assert.Equal(createdInterface.Id, response.ItInterfaceId);
                Assert.Equal(TestEnvironment.DefaultContractId, response.ItContractId);
            }
        }
    }
}
