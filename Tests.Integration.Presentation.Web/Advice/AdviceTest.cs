using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Core.DomainModel.Advice;
using Core.DomainModel.Organization;
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
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);

        }

        [Fact]
        public async Task Can_Add_Advice_With_Multiple_Email_Receievers()
        {
            //Arrange
            var recipient1 = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var recipient2 = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var recipient3 = CreateDefaultEmailRecipient(CreateWellformedEmail());

            var createAdvice = CreateDefaultAdvice(Scheduling.Day, A<AdviceType>(), recipient1);
            createAdvice.Reciepients.Add(recipient2);
            createAdvice.Reciepients.Add(recipient3);

            //Act
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

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
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

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
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

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
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

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
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

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
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

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
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

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
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

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
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Can_Delete_Inactive_Advice_That_Has_Not_Been_Sent()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var createAdvice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Repeat, recipient);

            var createResult = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);
            Assert.Equal(HttpStatusCode.Created, createResult.StatusCode);
            var createdAdvice = await createResult.ReadResponseBodyAsAsync<Core.DomainModel.Advice.Advice>();

            var deactivateResult = await AdviceHelper.DeactivateAdviceAsync(createdAdvice.Id);
            Assert.Equal(HttpStatusCode.NoContent, deactivateResult.StatusCode);

            //Act
            var deleteResult = await AdviceHelper.DeleteAdviceAsync(createdAdvice.Id);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_Delete_Active_Advice()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var createAdvice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Repeat, recipient);

            var createResult = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);
            Assert.Equal(HttpStatusCode.Created, createResult.StatusCode);
            var createdAdvice = await createResult.ReadResponseBodyAsAsync<Core.DomainModel.Advice.Advice>();

            //Act
            var deleteResult = await AdviceHelper.DeleteAdviceAsync(createdAdvice.Id);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_Delete_Advice_That_Does_Not_Exist()
        {
            //Arrange

            //Act
            var deleteResult = await AdviceHelper.DeleteAdviceAsync(0);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_Delete_Advice_That_Has_Been_Sent()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var createAdvice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Immediate, recipient);

            var createResult = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);
            Assert.Equal(HttpStatusCode.Created, createResult.StatusCode);
            var createdAdvice = await createResult.ReadResponseBodyAsAsync<Core.DomainModel.Advice.Advice>();

            Thread.Sleep(3000); //Sleep for 3 seconds to allow the immediate advis to be sent.

            //Act
            var deleteResult = await AdviceHelper.DeleteAdviceAsync(createdAdvice.Id);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_Delete_Inactive_Advice_That_Has_Not_Been_Sent_If_No_Rights()
        {
            //Arrange
            var recipient = CreateDefaultEmailRecipient(CreateWellformedEmail());
            var createAdvice = CreateDefaultAdvice(Scheduling.Day, AdviceType.Repeat, recipient);

            var createResult = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);
            Assert.Equal(HttpStatusCode.Created, createResult.StatusCode);
            var createdAdvice = await createResult.ReadResponseBodyAsAsync<Core.DomainModel.Advice.Advice>();

            var deactivateResult = await AdviceHelper.DeactivateAdviceAsync(createdAdvice.Id);
            Assert.Equal(HttpStatusCode.NoContent, deactivateResult.StatusCode);

            var regularUserCookie = await HttpApi.GetCookieAsync(OrganizationRole.User);

            //Act
            var deleteResult = await AdviceHelper.DeleteAdviceAsync(createdAdvice.Id, regularUserCookie);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, deleteResult.StatusCode);
        }

        private Core.DomainModel.Advice.Advice CreateDefaultAdvice(Scheduling schedule, AdviceType type, AdviceUserRelation recipient)
        {
            return new Core.DomainModel.Advice.Advice
            {
                RelationId = _root.Id,
                Type = ObjectType.itProject,
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
            return $"{A<string>()}@test.dk";
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
    }
}
