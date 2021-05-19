using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Advice;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Advice
{
    public class AdviceTest : WithAutoFixture
    {
        private const int OrganizationId = TestEnvironment.DefaultOrganizationId;

        [Fact]
        public async Task Can_Add_Advice()
        {
            //Arrange
            var recipient = createDefaultEmailRecipient(createWellformedEmail());
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Body = A<string>(),
                Subject = A<string>(),
                AdviceType = A<Core.DomainModel.Advice.AdviceType>(),
                Scheduling = Core.DomainModel.Advice.Scheduling.Day,
                Reciepients = new List<Core.DomainModel.Advice.AdviceUserRelation>()
                {
                    recipient
                },
                RelationId = A<int>(),
                AlarmDate = getRandomDate()
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
            var recipient1 = createDefaultEmailRecipient(createWellformedEmail());
            var recipient2 = createDefaultEmailRecipient(createWellformedEmail());
            var recipient3 = createDefaultEmailRecipient(createWellformedEmail());
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Body = A<string>(),
                Subject = A<string>(),
                AdviceType = A<Core.DomainModel.Advice.AdviceType>(),
                Scheduling = Core.DomainModel.Advice.Scheduling.Day,
                Reciepients = new List<Core.DomainModel.Advice.AdviceUserRelation>()
                {
                    recipient1,
                    recipient2,
                    recipient3
                },
                RelationId = A<int>(),
                AlarmDate = getRandomDate()
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
            var recipient = createDefaultEmailRecipient(A<string>()); // Malformed email
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Body = A<string>(),
                Subject = A<string>(),
                AdviceType = A<Core.DomainModel.Advice.AdviceType>(),
                Scheduling = Core.DomainModel.Advice.Scheduling.Day,
                Reciepients = new List<Core.DomainModel.Advice.AdviceUserRelation>()
                {
                    recipient
                },
                RelationId = A<int>(),
                AlarmDate = getRandomDate()
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
            var recipient = createDefaultEmailRecipient(createWellformedEmail());
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = Core.DomainModel.Advice.Scheduling.Day,
                AdviceType = AdviceType.Repeat,
                Reciepients = new List<Core.DomainModel.Advice.AdviceUserRelation>()
                {
                    recipient
                },
                RelationId = A<int>(),
                AlarmDate = DateTime.Now,
                StopDate = getRandomDate()
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
            var recipient = createDefaultEmailRecipient(createWellformedEmail());
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = Core.DomainModel.Advice.Scheduling.Day,
                AdviceType = AdviceType.Repeat,
                Reciepients = new List<Core.DomainModel.Advice.AdviceUserRelation>()
                {
                    recipient
                },
                RelationId = A<int>(),
                AlarmDate = DateTime.Now.AddDays(-1),
                StopDate = getRandomDate()
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
            var recipient = createDefaultEmailRecipient(createWellformedEmail());
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = Core.DomainModel.Advice.Scheduling.Day,
                AdviceType = AdviceType.Repeat,
                Reciepients = new List<Core.DomainModel.Advice.AdviceUserRelation>()
                {
                    recipient
                },
                RelationId = A<int>(),
                AlarmDate = null,
                StopDate = getRandomDate()
            };

            //Act
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Add_Repeatable_Advice_With_StopDate_Set_To_Null()
        {
            //Arrange
            var recipient = createDefaultEmailRecipient(createWellformedEmail());
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = Core.DomainModel.Advice.Scheduling.Day,
                AdviceType = AdviceType.Repeat,
                Reciepients = new List<Core.DomainModel.Advice.AdviceUserRelation>()
                {
                    recipient
                },
                RelationId = A<int>(),
                AlarmDate = DateTime.Now,
                StopDate = null
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
            var recipient = createDefaultEmailRecipient(createWellformedEmail());
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = Core.DomainModel.Advice.Scheduling.Day,
                AdviceType = AdviceType.Repeat,
                Reciepients = new List<Core.DomainModel.Advice.AdviceUserRelation>()
                {
                    recipient
                },
                RelationId = A<int>(),
                AlarmDate = DateTime.Now,
                StopDate = DateTime.Now.AddDays(-1),
            };

            //Act
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }


        private Core.DomainModel.Advice.AdviceUserRelation createDefaultEmailRecipient(string name)
        {
            return new Core.DomainModel.Advice.AdviceUserRelation
            {
                Name = name,
                RecieverType = Core.DomainModel.Advice.RecieverType.RECIEVER,
                RecpientType = Core.DomainModel.Advice.RecieverType.USER
            };
        }

        private string createWellformedEmail()
        {
            return $"{A<string>()}@test.dk";
        }

        private DateTime getRandomDate()
        {
            return DateTime.Now.AddDays(A<int>());
        }
    }
}
