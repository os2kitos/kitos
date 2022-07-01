using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Options.V1
{
    public class OptionApiTests : WithAutoFixture
    {
        private const int OrganizationId = TestEnvironment.DefaultOrganizationId;

        [Theory]
        [InlineData(EntityOptionHelper.ResourceNames.CriticalityTypes)]
        public async Task Can_Create_OptionType(string resourceName)
        {
            var name = A<string>();

            var response = await EntityOptionHelper.CreateOptionTypeAsync(resourceName, name, OrganizationId);

            Assert.Equal(response.Name, name);
        }

        [Theory]
        [InlineData(EntityOptionHelper.ResourceNames.CriticalityTypes)]
        public async Task Can_Change_OptionType_Name(string resourceName)
        {
            var name = A<string>();
            var newName = A<string>();

            var option = await EntityOptionHelper.CreateOptionTypeAsync(resourceName, name, OrganizationId);
            var response = await EntityOptionHelper.ChangeOptionTypeNameAsync(resourceName, option.Id, newName);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
