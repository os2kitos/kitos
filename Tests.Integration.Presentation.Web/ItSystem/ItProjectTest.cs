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

            //Act - perform the action with the actual role
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

            //Act - perform the action with the actual role
            using (var result = await ItProjectHelper.SendAddGoalRequestAsync(_project.Id, humanReadableId, measurable, name, description, goalDate1, goalDate2, goalDate3, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Can_Add_Assignment(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var description = A<string>();
            var name = A<string>();
            var note = A<string>();
            var statusPercentage = A<int>() % 100;
            var timeEstimate = A<int>();
            var startDate = A<DateTime>().Date;
            var endDate = startDate.AddDays(10);

            //Act - perform the action with the actual role
            var result = await ItProjectHelper.AddAssignmentAsync(OrganizationId, _project.Id, description, name, note, statusPercentage, timeEstimate, startDate, endDate, login);

            //Assert
            Assert.Equal(_project.Id, result.AssociatedItProjectId.GetValueOrDefault());
            Assert.Equal(description, result.Description);
            Assert.Equal(name, result.Name);
            Assert.Equal(note, result.Note);
            Assert.Equal(statusPercentage, result.StatusProcentage);
            Assert.Equal(timeEstimate, result.TimeEstimate);
            Assert.Equal(startDate, result.StartDate);
            Assert.Equal(endDate, result.EndDate);

        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Add_Assignment(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);

            //Act - perform the action with the actual role
            var description = A<string>();
            var name = A<string>();
            var note = A<string>();
            var statusPercentage = A<int>() % 100;
            var timeEstimate = A<int>();
            var startDate = A<DateTime>().Date;
            var endDate = startDate.AddDays(10);

            //Act - perform the action with the actual role
            using (var result = await ItProjectHelper.SendAddAssignmentRequestAsync(OrganizationId, _project.Id, description, name, note, statusPercentage, timeEstimate, startDate, endDate, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Can_Add_MileStone(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var description = A<string>();
            var name = A<string>();
            var note = A<string>();
            var timeEstimate = A<int>();
            var humanReadableId = A<string>();
            var date = A<DateTime>().Date;

            //Act - perform the action with the actual role
            var result = await ItProjectHelper.AddMileStoneAsync(OrganizationId, _project.Id, description, name, note, humanReadableId, timeEstimate, date, login);

            //Assert
            Assert.Equal(_project.Id, result.AssociatedItProjectId.GetValueOrDefault());
            Assert.Equal(description, result.Description);
            Assert.Equal(name, result.Name);
            Assert.Equal(note, result.Note);
            Assert.Equal(timeEstimate, result.TimeEstimate);
            Assert.Equal(date, result.Date.Value);
            Assert.Equal(humanReadableId, result.HumanReadableId);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Add_MileStone(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);

            var description = A<string>();
            var name = A<string>();
            var note = A<string>();
            var timeEstimate = A<int>();
            var humanReadableId = A<string>();
            var date = A<DateTime>().Date;

            //Act - perform the action with the actual role
            using (var result = await ItProjectHelper.SendAddMileStoneRequestAsync(OrganizationId, _project.Id, description, name, note, humanReadableId, timeEstimate, date, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Can_Add_Stakeholder(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);

            //Act - perform the action with the actual role
            var howToHandle = A<string>();
            var name = A<string>();
            var roleName = A<string>();
            var downsides = howToHandle;
            var benefits = howToHandle;
            var significance = A<int>() % 5;

            var result = await ItProjectHelper.AddStakeholderAsync(_project.Id, name, roleName, downsides, benefits, howToHandle, significance, login);

            //Assert
            Assert.Equal(_project.Id, result.ItProjectId);
            Assert.Equal(name, result.Name);
            Assert.Equal(roleName, result.Role);
            Assert.Equal(downsides, result.Downsides);
            Assert.Equal(benefits, result.Benefits);
            Assert.Equal(howToHandle, result.HowToHandle);
            Assert.Equal(significance, result.Significance);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Add_Stakeholder(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);

            //Act - perform the action with the actual role
            var howToHandle = A<string>();
            var name = A<string>();
            var roleName = A<string>();
            var downsides = howToHandle;
            var benefits = howToHandle;
            var significance = A<int>() % 5;

            //Act - perform the action with the actual role
            using (var result = await ItProjectHelper.SendAddStakeholderRequestAsync(_project.Id, name, roleName, downsides, benefits, howToHandle, significance, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            }
        }
    }
}
