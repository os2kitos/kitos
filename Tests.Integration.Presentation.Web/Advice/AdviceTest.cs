using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Core.DomainModel.Advice;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Core.DomainServices.Extensions;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Advice
{
    public class AdviceTest : WithAutoFixture, IAsyncLifetime
    {
        private ItProjectDTO _root;
        private const int OrganizationId = TestEnvironment.DefaultOrganizationId;

        [Fact]
        public async Task Can_Add_Advice()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());

            var createAdvice = CreateDefaultAdvice(Scheduling.Day, A<AdviceType>(), recipient);

            //Act
            using var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);

        }

        [Fact]
        public async Task Can_Add_Advice_With_Multiple_Email_Receivers()
        {
            //Arrange
            var recipient1 = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var recipient2 = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var recipient3 = CreateDefaultEmailRecipient(CreateWellformedEmail());

            var createAdvice = CreateDefaultAdvice(Scheduling.Day, A<AdviceType>(), recipient1);
            createAdvice.Reciepients.Add(recipient2);
            createAdvice.Reciepients.Add(recipient3);

            //Act
            using var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);

        }

        [Fact]
        public async Task Cannot_Add_Advice_When_Emails_Are_Malformed()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(A<string>()); // Malformed email
            var createAdvice = CreateDefaultAdvice(Scheduling.Day, A<AdviceType>(), recipient);

            //Act
            using var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }


        [Fact]
        public async Task Can_Add_Repeatable_Advice_With_StartDate_Today()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var createAdvice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Repeat, recipient);

            //Act
            using var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public async Task Can_Add_Repeatable_Advice_With_StartDate_After_Today()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var createAdvice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Repeat, recipient);
            createAdvice.AlarmDate = GetRandomDateAfterToday();
            createAdvice.StopDate = createAdvice.AlarmDate.Value.AddDays(1);

            //Act
            using var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Add_Repeatable_Advice_With_StartDate_Before_Today()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var createAdvice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Repeat, recipient);
            createAdvice.AlarmDate = DateTime.Now.AddDays(-1);

            //Act
            using var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Add_Advice_If_No_Type_Defined()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());

            var createAdvice = CreateDefaultAdvice(Scheduling.Day, A<AdviceType>(), recipient);
            createAdvice.Type = null;

            //Act
            using var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Add_Advice_If_No_RelationId_Defined()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());

            var createAdvice = CreateDefaultAdvice(Scheduling.Day, A<AdviceType>(), recipient);
            createAdvice.RelationId = null;

            //Act
            using var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        }

        [Fact]
        public async Task Cannot_Add_Repeatable_Advice_With_StartDate_Set_To_Null()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var createAdvice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Repeat, recipient);
            createAdvice.AlarmDate = null;

            //Act
            using var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Add_Repeatable_Advice_With_StopDate_Before_StartDate()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var createAdvice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Repeat, recipient);
            createAdvice.StopDate = DateTime.Now.AddDays(-1);

            //Act
            using var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Can_Delete_Inactive_Advice_That_Has_Not_Been_Sent()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var advice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Repeat, recipient);
            advice.AlarmDate = DateTime.Now.AddDays(2);
            advice.StopDate = DateTime.Now.AddDays(3);

            using var createResult = await AdviceHelper.PostAdviceAsync(advice, OrganizationId);
            Assert.Equal(HttpStatusCode.Created, createResult.StatusCode);
            var createdAdvice = await createResult.ReadResponseBodyAsAsync<Core.DomainModel.Advice.Advice>();

            using var deactivateResult = await AdviceHelper.DeactivateAdviceAsync(createdAdvice.Id);
            Assert.Equal(HttpStatusCode.NoContent, deactivateResult.StatusCode);

            //Act
            using var deleteResult = await AdviceHelper.DeleteAdviceAsync(createdAdvice.Id);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_Delete_Active_Advice()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var createAdvice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Repeat, recipient);

            using var createResult = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);
            Assert.Equal(HttpStatusCode.Created, createResult.StatusCode);
            var createdAdvice = await createResult.ReadResponseBodyAsAsync<Core.DomainModel.Advice.Advice>();

            //Act
            using var deleteResult = await AdviceHelper.DeleteAdviceAsync(createdAdvice.Id);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_Delete_Advice_That_Does_Not_Exist()
        {
            //Arrange

            //Act
            using var deleteResult = await AdviceHelper.DeleteAdviceAsync(0);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_Delete_Advice_That_Has_Been_Sent()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var createAdvice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Immediate, recipient);

            using var createResult = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);
            Assert.Equal(HttpStatusCode.Created, createResult.StatusCode);
            var createdAdvice = await createResult.ReadResponseBodyAsAsync<Core.DomainModel.Advice.Advice>();

            //Wait for the advice to have been sent
            await WaitForAsync(() => Task.FromResult(DatabaseAccess.MapFromEntitySet<Core.DomainModel.Advice.Advice, bool>(advices => advices.AsQueryable().ById(createdAdvice.Id).AdviceSent.Any())), TimeSpan.FromSeconds(10));


            //Act
            using var deleteResult = await AdviceHelper.DeleteAdviceAsync(createdAdvice.Id);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_Delete_Inactive_Advice_That_Has_Not_Been_Sent_If_No_Rights()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var advice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Repeat, recipient);

            using var createResult = await AdviceHelper.PostAdviceAsync(advice, OrganizationId);
            Assert.Equal(HttpStatusCode.Created, createResult.StatusCode);
            var createdAdvice = await createResult.ReadResponseBodyAsAsync<Core.DomainModel.Advice.Advice>();

            using var deactivateResult = await AdviceHelper.DeactivateAdviceAsync(createdAdvice.Id);
            Assert.Equal(HttpStatusCode.NoContent, deactivateResult.StatusCode);

            var regularUserCookie = await HttpApi.GetCookieAsync(OrganizationRole.User);

            //Act
            using var deleteResult = await AdviceHelper.DeleteAdviceAsync(createdAdvice.Id, regularUserCookie);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Write_Access_Business_Role_To_Owner_Root_Provides_Write_Access_To_Advices()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(registration.Id);
            var writeAccessRole = businessRoleDtos.First(x => x.HasWriteAccess);
            var availableUsers = await DataProcessingRegistrationHelper.GetAvailableUsersAsync(registration.Id, writeAccessRole.Id);
            var readOnlyUserCredentials = TestEnvironment.GetCredentials(OrganizationRole.User);
            var readOnlyUser = availableUsers.First(x => x.Email.Equals(readOnlyUserCredentials.Username, StringComparison.OrdinalIgnoreCase));
            var readOnlyUserCookie = await HttpApi.GetCookieAsync(OrganizationRole.User);
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var advice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Repeat, recipient);
            advice.RelationId = registration.Id;
            advice.Type = RelatedEntityType.dataProcessingRegistration;

            // ****************************************
            // ************* Act + assert ************* 
            // ****************************************
            // Before role has been assigned the user has no write access to the root
            await AssertAdviceCreationReturns(advice, readOnlyUserCookie, HttpStatusCode.Forbidden);

            using var assignResponse = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(registration.Id, writeAccessRole.Id, readOnlyUser.Id);

            // With the write-access role assigned, the user should be allowed to create advices (modify the root)
            await AssertAdviceCreationReturns(advice, readOnlyUserCookie, HttpStatusCode.Created);

            using var response = await DataProcessingRegistrationHelper.SendRemoveRoleRequestAsync(registration.Id, writeAccessRole.Id, readOnlyUser.Id);

            //Removing the role should revert the assigned write access
            await AssertAdviceCreationReturns(advice, readOnlyUserCookie, HttpStatusCode.Forbidden);

        }

        private static async Task AssertAdviceCreationReturns(Core.DomainModel.Advice.Advice advice, Cookie readOnlyUserCookie, HttpStatusCode expectedResult)
        {
            using var createResultBeforeRoleAssignment = await AdviceHelper.PostAdviceAsync(advice, OrganizationId, readOnlyUserCookie);
            Assert.Equal(expectedResult, createResultBeforeRoleAssignment.StatusCode);
        }

        private Core.DomainModel.Advice.Advice CreateDefaultAdvice(Scheduling schedule, AdviceType type, AdviceUserRelation recipient)
        {
            return new Core.DomainModel.Advice.Advice
            {
                RelationId = _root.Id,
                Type = RelatedEntityType.itProject,
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = schedule,
                AdviceType = type,
                Reciepients = new List<AdviceUserRelation>()
                {
                    recipient
                },
                AlarmDate = DateTime.Now,
                StopDate = GetRandomDateAfterToday(),
            };
        }


        private AdviceUserRelation CreateDefaultEmailRecipient(string name)
        {
            return new AdviceUserRelation
            {
                Name = name,
                RecieverType = RecieverType.RECIEVER,
                RecpientType = RecieverType.USER
            };
        }

        private string CreateWellformedEmail()
        {
            //Make sure special chars are part of the test email
            return $"{A<string>()}_a.b-c@test.dk";
        }

        private DateTime GetRandomDateAfterToday()
        {
            return DateTime.Now.AddDays(Math.Abs(A<int>()));
        }

        public async Task InitializeAsync()
        {
            _root = await ItProjectHelper.CreateProject(A<string>(), OrganizationId);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        private static async Task WaitForAsync(Func<Task<bool>> check, TimeSpan howLong)
        {
            bool conditionMet;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
                conditionMet = await check();
            } while (conditionMet == false && stopwatch.Elapsed <= howLong);

            Assert.True(conditionMet, $"Failed to meet required condition within {howLong.TotalMilliseconds} milliseconds");
        }
    }
}
