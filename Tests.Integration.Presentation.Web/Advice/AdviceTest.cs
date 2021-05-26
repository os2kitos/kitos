using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Advice;
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
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Type = ObjectType.itProject,
                RelationId = _root.Id,
                Body = A<string>(),
                Subject = A<string>(),
                AdviceType = A<AdviceType>(),
                Scheduling = Scheduling.Day,
                Reciepients = new List<AdviceUserRelation>()
                {
                    recipient
                },
                AlarmDate = GetRandomDateAfterToday()
            };

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
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Type = ObjectType.itProject,
                RelationId = _root.Id,
                Body = A<string>(),
                Subject = A<string>(),
                AdviceType = A<AdviceType>(),
                Scheduling = Scheduling.Day,
                Reciepients = new List<AdviceUserRelation>()
                {
                    recipient1,
                    recipient2,
                    recipient3
                },
                AlarmDate = GetRandomDateAfterToday()
            };

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
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Type = ObjectType.itProject,
                RelationId = _root.Id,
                Body = A<string>(),
                Subject = A<string>(),
                AdviceType = A<AdviceType>(),
                Scheduling = Scheduling.Day,
                Reciepients = new List<AdviceUserRelation>()
                {
                    recipient
                },
                AlarmDate = GetRandomDateAfterToday()
            };

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
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Type = ObjectType.itProject,
                RelationId = _root.Id,
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = Scheduling.Day,
                AdviceType = AdviceType.Repeat,
                Reciepients = new List<AdviceUserRelation>()
                {
                    recipient
                },
                AlarmDate = DateTime.Now,
                StopDate = GetRandomDateAfterToday()
            };

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
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Type = ObjectType.itProject,
                RelationId = _root.Id,
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = Scheduling.Day,
                AdviceType = AdviceType.Repeat,
                Reciepients = new List<AdviceUserRelation>()
                {
                    recipient
                },
                AlarmDate = DateTime.Now.AddDays(-1),
                StopDate = GetRandomDateAfterToday()
            };

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
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Type = ObjectType.itProject,
                RelationId = _root.Id,
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = Scheduling.Day,
                AdviceType = AdviceType.Repeat,
                Reciepients = new List<AdviceUserRelation>()
                {
                    recipient
                },
                AlarmDate = null,
                StopDate = GetRandomDateAfterToday()
            };

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
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Type = ObjectType.itProject,
                RelationId = _root.Id,
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = Scheduling.Day,
                AdviceType = AdviceType.Repeat,
                Reciepients = new List<AdviceUserRelation>()
                {
                    recipient
                },
                AlarmDate = DateTime.Now,
                StopDate = DateTime.Now.AddDays(-1),
            };

            //Act
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }


        private static AdviceUserRelation CreateDefaultEmailRecipient(string name)
        {
            return new()
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
