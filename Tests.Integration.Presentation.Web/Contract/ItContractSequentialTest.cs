using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.ItContract;
using Presentation.Web.Models.API.V1.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Contract
{
    [Collection(nameof(SequentialTestGroup))]
    public class ItContractSequentialTest : WithAutoFixture
    {
        private string CreateName()
        {
            return $"{nameof(ItContractSequentialTest)}{A<string>()}";
        }

        [Fact]
        public async Task Can_Get_Available_Options()
        {
            //Arrange
            var globalAdminCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var organizationDto = await CreateOrganizationAsync();

            //Act
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl("api/itcontract/available-options-in/" + organizationDto.Id), globalAdminCookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var dto = await response.ReadResponseBodyAsKitosApiResponseAsync<ContractOptionsDTO>();

            AssertOptionSet<CriticalityType>(dto.CriticalityOptions);
            AssertOptionSet<ItContractType>(dto.ContractTypeOptions);
            AssertOptionSet<ItContractTemplateType>(dto.ContractTemplateOptions);
            AssertOptionSet<PurchaseFormType>(dto.PurchaseFormOptions);
            AssertOptionSet<ProcurementStrategyType>(dto.ProcurementStrategyOptions);
            AssertOptionSet<PaymentModelType>(dto.PaymentModelOptions);
            AssertOptionSet<PaymentFreqencyType>(dto.PaymentFrequencyOptions);
            AssertOptionSet<OptionExtendType>(dto.OptionExtendOptions);
            AssertOptionSet<TerminationDeadlineType>(dto.TerminationDeadlineOptions);
        }

        private static void AssertOptionSet<TOption>(IEnumerable<OptionWithDescriptionAndExpirationDTO> dtoResult) where TOption : class, IEntity
        {
            var dbIds = DatabaseAccess.MapFromEntitySet<TOption, IEnumerable<int>>(x => x.AsQueryable().Select(o => o.Id).ToList());
            Assert.Equal(dbIds.OrderBy(x => x), dtoResult.Select(x => x.Id).OrderBy(x => x));
        }

        private async Task<OrganizationDTO> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, organizationName, "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            return organization;
        }
    }
}
