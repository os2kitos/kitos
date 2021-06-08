using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Advice;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using ExpectedObjects;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Contract
{
    public class ItContractTest : WithAutoFixture
    {
        private const int OrganizationId = TestEnvironment.DefaultOrganizationId;

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Can_Add_HandoverTrial(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var contract = await ItContractHelper.CreateContract(A<string>(), OrganizationId);
            var approved = A<DateTime>().Date;
            var expected = A<DateTime>().Date;

            //Act - perform the action with the actual role
            var result = await ItContractHelper.AddHandOverTrialAsync(contract.OrganizationId, contract.Id, approved, expected, login);

            //Assert
            Assert.Equal(contract.Id, result.ItContractId);
            Assert.Equal(approved, result.Approved.GetValueOrDefault());
            Assert.Equal(expected, result.Expected.GetValueOrDefault());
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Add_HandoverTrial(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var contract = await ItContractHelper.CreateContract(A<string>(), OrganizationId);
            var approved = A<DateTime>().Date;
            var expected = A<DateTime>().Date;

            //Act - perform the action with the actual role
            using (var result = await ItContractHelper.SendAddHandOverTrialAsync(contract.OrganizationId, contract.Id, approved, expected, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Can_Add_PaymentMilestone(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var contract = await ItContractHelper.CreateContract(A<string>(), OrganizationId);
            var approved = A<DateTime>().Date;
            var expected = A<DateTime>().Date;
            var title = A<string>();

            //Act - perform the action with the actual role
            var result = await ItContractHelper.AddPaymentMilestoneAsync(contract.OrganizationId, contract.Id, approved, expected, title, login);

            //Assert
            Assert.Equal(contract.Id, result.ItContractId);
            Assert.Equal(approved, result.Approved.GetValueOrDefault());
            Assert.Equal(expected, result.Expected.GetValueOrDefault());
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Add_PaymentMilestone(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var contract = await ItContractHelper.CreateContract(A<string>(), OrganizationId);
            var approved = A<DateTime>().Date;
            var expected = A<DateTime>().Date;
            var title = A<string>();

            //Act - perform the action with the actual role
            using (var result = await ItContractHelper.SendAddPaymentMilestoneRequestAsync(contract.OrganizationId, contract.Id, approved, expected, title, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            }
        }

        [Fact]
        public async Task Can_Get_Available_DataProcessingRegistrations()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var registrationName = A<string>();
            var registration1 = await DataProcessingRegistrationHelper.CreateAsync(organizationId, registrationName + "1");
            var registration2 = await DataProcessingRegistrationHelper.CreateAsync(organizationId, registrationName + "2");
            var contract = await ItContractHelper.CreateContract(A<string>(), OrganizationId);

            //Act
            var dtos = (await ItContractHelper.GetAvailableDataProcessingRegistrationsAsync(contract.Id, registrationName)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            dtos.Select(x => new { x.Id, x.Name }).ToExpectedObject().ShouldMatch(new[] { new { registration1.Id, registration1.Name }, new { registration2.Id, registration2.Name } });
        }

        [Fact]
        public async Task Can_Assign_And_Remove_DataProcessingRegistration()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var registration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, A<string>());
            var contract = await ItContractHelper.CreateContract(A<string>(), OrganizationId);

            //Act - Add
            using var assignResponse = await ItContractHelper.SendAssignDataProcessingRegistrationAsync(contract.Id, registration.Id);
            using var duplicateResponse = await ItContractHelper.SendAssignDataProcessingRegistrationAsync(contract.Id, registration.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
            var dto = await ItContractHelper.GetItContract(contract.Id);
            var namedEntityDTO = Assert.Single(dto.DataProcessingRegistrations);
            Assert.Equal(registration.Id, namedEntityDTO.Id);
            Assert.Equal(registration.Name, namedEntityDTO.Name);

            //Act - remove
            using var removeResponse = await ItContractHelper.SendRemoveDataProcessingRegistrationAsync(contract.Id, registration.Id);
            using var duplicateRemoveResponse = await ItContractHelper.SendRemoveDataProcessingRegistrationAsync(contract.Id, registration.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, duplicateRemoveResponse.StatusCode);
            dto = await ItContractHelper.GetItContract(contract.Id);
            Assert.Empty(dto.DataProcessingRegistrations);
        }

        [Fact]
        public async Task Delete_Contract_Removes_Associated_Advices()
        {
            // Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var contract = await ItContractHelper.CreateContract(A<string>(), OrganizationId);
            var advice = new Core.DomainModel.Advice.Advice
            {
                Id = A<int>(),
                AlarmDate = DateTime.Now,
                StopDate = DateTime.Now.AddDays(365),
                Scheduling = Scheduling.Quarter,
                Subject = "Can_Delete_Contract_Advices",
                RelationId = contract.Id,
                Type = RelatedEntityType.itProject
            };
            using var createResponse = await AdviceHelper.PostAdviceAsync(advice, organizationId);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Act
            using var deleteResponse = await ItContractHelper.SendDeleteContractRequestAsync(contract.Id);
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Assert
            using var advicesResponse = AdviceHelper.GetContractAdvicesAsync(contract.Id);
            var deletedContractAdvices = await advicesResponse.Result.ReadOdataListResponseBodyAsAsync<Core.DomainModel.Advice.Advice>();
            Assert.True(deletedContractAdvices.Count == 0);
        }
    }
}
