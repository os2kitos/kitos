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
    public class ItContractSequentialTest : WithAutoFixture, IAsyncLifetime
    {
        private OrganizationDTO _organizationDto;

        private string CreateName()
        {
            return $"{nameof(ItContractSequentialTest)}{A<string>()}";
        }

        [Fact]
        public async Task Can_Get_Available_Options()
        {
            //Arrange
            var globalAdminCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            //Act
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl("api/itcontract/available-options-in/" + _organizationDto.Id), globalAdminCookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var dto = await response.ReadResponseBodyAsKitosApiResponseAsync<ContractOptionsDTO>();

            await AssertOptionSet<CriticalityType>(dto.CriticalityOptions, LocalOptionHelper.ResourceNames.LocalCriticalityTypes);
            await AssertOptionSet<ItContractType>(dto.ContractTypeOptions, LocalOptionHelper.ResourceNames.LocalItContractTypes);
            await AssertOptionSet<ItContractTemplateType>(dto.ContractTemplateOptions, LocalOptionHelper.ResourceNames.LocalItContractTemplateTypes);
            await AssertOptionSet<PurchaseFormType>(dto.PurchaseFormOptions, LocalOptionHelper.ResourceNames.LocalPurchaseFormTypes);
            await AssertOptionSet<ProcurementStrategyType>(dto.ProcurementStrategyOptions, LocalOptionHelper.ResourceNames.LocalProcurementStrategyTypes);
            await AssertOptionSet<PaymentModelType>(dto.PaymentModelOptions, LocalOptionHelper.ResourceNames.LocalPaymentModelTypes);
            await AssertOptionSet<PaymentFreqencyType>(dto.PaymentFrequencyOptions, LocalOptionHelper.ResourceNames.LocalPaymentFrequencyTypes);
            await AssertOptionSet<OptionExtendType>(dto.OptionExtendOptions, LocalOptionHelper.ResourceNames.LocalOptionExtendTypes);
            await AssertOptionSet<TerminationDeadlineType>(dto.TerminationDeadlineOptions, LocalOptionHelper.ResourceNames.LocalTerminationDeadlineTypes);
        }

        private async Task AssertOptionSet<TOption>(IEnumerable<OptionWithDescriptionAndExpirationDTO> dtoResult, string localResourceType) where TOption : class, IEntity
        {
            var dtos = dtoResult.ToList();
            //Check that the option set is complete
            var dbIds = DatabaseAccess
                .MapFromEntitySet<TOption, IEnumerable<int>>(x => x.AsQueryable().Select(o => o.Id)
                    .OrderBy(id => id)
                    .ToList());
            Assert.Equal(dbIds, dtos.Select(x => x.Id).OrderBy(x => x).ToList());

            //Check that availability is the same as reported by local options api
            var localOptions = await LocalOptionHelper.GetOptionsAsync<ItContract>(localResourceType, _organizationDto.Id);
            var locallyAvailableOptionIds = localOptions
                .Where(x => x.IsLocallyAvailable || x.IsObligatory)
                .Select(optionEntity => optionEntity.Id)
                .OrderBy(id => id)
                .ToList();

            var activeIdsFromDtoResult = dtos
                .Where(dto => !dto.Expired)
                .Select(dto => dto.Id)
                .OrderBy(id => id)
                .ToList();

            Assert.Equal(locallyAvailableOptionIds, activeIdsFromDtoResult);

        }

        private async Task<OrganizationDTO> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, organizationName, "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            return organization;
        }

        public async Task InitializeAsync()
        {
            _organizationDto = await CreateOrganizationAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
