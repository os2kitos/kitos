using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
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
    }
}
