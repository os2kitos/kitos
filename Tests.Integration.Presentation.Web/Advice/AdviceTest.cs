using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
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
            var recipient = createDefaultEmailRecipient();
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = Core.DomainModel.Advice.Scheduling.Day,
                Reciepients = new List<Core.DomainModel.Advice.AdviceUserRelation>()
                {
                    recipient
                },
                RelationId = A<int>(),
                AlarmDate = A<DateTime>()
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
            var recipient1 = createDefaultEmailRecipient();
            var recipient2 = createDefaultEmailRecipient();
            var recipient3 = createDefaultEmailRecipient();
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = Core.DomainModel.Advice.Scheduling.Day,
                Reciepients = new List<Core.DomainModel.Advice.AdviceUserRelation>()
                {
                    recipient1,
                    recipient2,
                    recipient3
                },
                RelationId = A<int>(),
                AlarmDate = A<DateTime>()
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
            var recipient = createDefaultEmailRecipient();
            var createAdvice = new Core.DomainModel.Advice.Advice
            {
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = Core.DomainModel.Advice.Scheduling.Day,
                Reciepients = new List<Core.DomainModel.Advice.AdviceUserRelation>()
                {
                    recipient
                },
                RelationId = A<int>(),
                AlarmDate = A<DateTime>()
            };

            //Act
            var result = await AdviceHelper.PostAdviceAsync(createAdvice, OrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        private Core.DomainModel.Advice.AdviceUserRelation createDefaultEmailRecipient()
        {
            return new Core.DomainModel.Advice.AdviceUserRelation
            {
                Name = A<string>(), //Malformed email
                RecieverType = Core.DomainModel.Advice.RecieverType.RECIEVER,
                RecpientType = Core.DomainModel.Advice.RecieverType.USER
            };
        }

        private string createWellformedEmail()
        {
            return $"{A<string>()}@test.dk";
        }
    }
}
