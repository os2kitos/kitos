using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Advice;
using Core.DomainModel.Shared;
using Core.DomainServices.Extensions;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Xunit;

namespace Tests.Integration.Presentation.Web.Advice
{
    [Collection(nameof(SequentialTestGroup))]
    public class AdviceSequentialTest : AdviceTestBase
    {
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
            await WaitForAsync(() => Task.FromResult(DatabaseAccess.MapFromEntitySet<Core.DomainModel.Advice.Advice, bool>(advices => advices.AsQueryable().ById(createdAdvice.Id).AdviceSent.Any())), TimeSpan.FromSeconds(120));


            //Act
            using var deleteResult = await AdviceHelper.DeleteAdviceAsync(createdAdvice.Id);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, deleteResult.StatusCode);
        }

        private Core.DomainModel.Advice.Advice CreateDefaultAdvice(Scheduling schedule, AdviceType type, AdviceUserRelation recipient)
        {
            return new Core.DomainModel.Advice.Advice
            {
                RelationId = Root.Id,
                Type = RelatedEntityType.itContract,
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
    }
}
