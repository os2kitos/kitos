using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Advice;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using ExpectedObjects;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Extensions;
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
        public async Task Can_Add_CriticalityType(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var contract = await ItContractHelper.CreateContract(A<string>(), OrganizationId);
            var criticality = (await EntityOptionHelper.GetOptionsAsync(EntityOptionHelper.ResourceNames.CriticalityTypes, OrganizationId)).RandomItem();

            //Act - perform the action with the actual role
            await ItContractHelper.AssignCriticalityTypeAsync(contract.OrganizationId, contract.Id, criticality.Id, login);
            var contractResult = await ItContractHelper.GetItContract(contract.Id);

            //Assert
            Assert.Equal(contract.Id, contractResult.Id);
            Assert.Equal(criticality.Id, contractResult.CriticalityId);
            Assert.Equal(criticality.Name, contractResult.CriticalityName);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Add_Criticality(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var contract = await ItContractHelper.CreateContract(A<string>(), OrganizationId);
            var criticality = (await EntityOptionHelper.GetOptionsAsync(EntityOptionHelper.ResourceNames.CriticalityTypes, OrganizationId)).RandomItem();

            //Act - perform the action with the actual role
            using var result = await ItContractHelper.SendAssignCriticalityTypeAsync(contract.OrganizationId, contract.Id, criticality.Id, login);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
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
                Type = RelatedEntityType.itContract
            };
            using var createResponse = await AdviceHelper.PostAdviceAsync(advice, organizationId);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Act
            using var deleteResponse = await ItContractHelper.SendDeleteContractRequestAsync(contract.Id);
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Assert
            using var advicesResponse =await AdviceHelper.GetContractAdvicesAsync(organizationId, contract.Id);
            var deletedContractAdvices = await advicesResponse.ReadOdataListResponseBodyAsAsync<Core.DomainModel.Advice.Advice>();
            Assert.Empty(deletedContractAdvices);
        }

        [Fact]
        public async Task Cannot_Create_With_Duplicate_Name_In_Same_Organization()
        {
            //Arrange
            var name = A<string>();
            using var r1 = await ItContractHelper.SendCreateContract(name, TestEnvironment.DefaultOrganizationId).WithExpectedResponseCode(HttpStatusCode.Created);

            //Act
            using var conflictedResponse = await ItContractHelper.SendCreateContract(name, TestEnvironment.DefaultOrganizationId);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, conflictedResponse.StatusCode);
        }

        [Theory]
        [InlineData(false, false, true)]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        public async Task Can_Get_Validation_Details(bool expectDateError, bool enforceValid, bool expectValid)
        {
            //Arrange
            var itContractDto = await ItContractHelper.CreateContract(A<string>(), TestEnvironment.DefaultOrganizationId);
            await ItContractHelper.PatchContract(itContractDto.Id, TestEnvironment.DefaultOrganizationId, new
            {
                Active = enforceValid,
                Concluded = DateTime.Now.AddDays(expectDateError ? 1 : -1)
            });

            //Act
            var result = await ItContractHelper.GetItContractValidationDetailsAsync(itContractDto.Id);

            //Assert
            Assert.Equal(expectValid, result.Valid);
            Assert.Equal(enforceValid, result.EnforcedValid);
            if (expectDateError)
            {
                var dateError = Assert.Single(result.Errors);
                Assert.Equal(ItContractValidationError.StartDateNotPassed, dateError);
            }
            else
            {
                Assert.Empty(result.Errors);
            }
        }
    }
}
