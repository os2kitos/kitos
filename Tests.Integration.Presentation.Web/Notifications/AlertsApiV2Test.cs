using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V2.Request.User;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Internal.Notifications;
using Tests.Integration.Presentation.Web.Tools.Internal.Users;
using Xunit;

namespace Tests.Integration.Presentation.Web.Notifications
{
    public class AlertsApiV2Test : BaseTest
    {
        [Fact]
        public async Task Can_Not_Get_User_Alerts_If_Not_Current_User()
        {
            var org = await CreateOrganizationAsync();
            var userRequest = A<CreateUserRequestDTO>();
            userRequest.Email = $"{A<string>()}@{A<string>()}.dk";
            var user = await UsersV2Helper.CreateUser(org.Uuid, userRequest);

            var response = await AlertsV2Helper.GetAlertsAsync(org.Uuid, user.Uuid, RelatedEntityType.itSystemUsage);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Delete_Returns_Not_Found_If_Invalid_Guid()
        {
            var invalidGuid = A<Guid>();

            var response = await AlertsV2Helper.DeleteAlertAsync(invalidGuid);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
