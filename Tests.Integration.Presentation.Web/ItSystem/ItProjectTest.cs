using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItProjectTest : WithAutoFixture, IAsyncLifetime
    {
        private ItProjectDTO _project;
        private const int OrganizationId = TestEnvironment.DefaultOrganizationId;

        /*
         *It Projekt

            - Lokal admin kan ikke oprette mål: status: Mål. Har ikke rettigheder
            - Lokal admin kan ikke oprette opgave: Status projekt. Har ikke rettigheder
            - Lokal admin kan ikke oprette milepæl: Status projekt. Har ikke rettigheder
            - Lokal admin kan ikke oprette interessenter: Interessenter. Har ikke rettigheder
            - Lokal admin kan ikke oprette risiko: risiko. Har ikke rettigheder
            - Lokal admin kan ikke oprette kommunikation: kommunikation. Har ikke rettigheder
         *
         */
        public async Task InitializeAsync()
        {
            _project = await ItProjectHelper.CreateProject(A<string>(), OrganizationId);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Can_Add_Goal(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var humanReadableId = A<string>();
            var measurable = A<bool>();
            var name = A<string>();
            var description = A<string>();
            var goalDate1 = A<DateTime>().Date;
            var goalDate2 = A<DateTime>().Date;
            var goalDate3 = A<DateTime>().Date;

            //Act - perform the POST with the actual role
            var result = await ItProjectHelper.AddGoalAsync(_project.Id, humanReadableId, measurable, name, description, goalDate1, goalDate2, goalDate3, login);

            //Assert
            Assert.Equal(_project.Id, result.GoalStatusId);
            Assert.Equal(description, result.Description);
            Assert.Equal(name, result.Name);
            Assert.Equal(humanReadableId, result.HumanReadableId);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Add_Goal(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var humanReadableId = A<string>();
            var measurable = A<bool>();
            var name = A<string>();
            var description = A<string>();
            var goalDate1 = A<DateTime>().Date;
            var goalDate2 = A<DateTime>().Date;
            var goalDate3 = A<DateTime>().Date;

            //Act - perform the POST with the actual role
            using (var result = await ItProjectHelper.SendAddGoalAsyncRequestAsync(_project.Id, humanReadableId, measurable, name, description, goalDate1, goalDate2, goalDate3, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            }
        }
    }
}
