using System;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Xunit;

namespace Tests.Integration.Presentation.Web.Contract
{
    [Collection(nameof(SequentialTestGroup))]
    public class ItContractSupplierOverviewReadModelsApiTest : BaseTest, IAsyncLifetime
    {
        private ShallowOrganizationResponseDTO _organization = null!;

        public async Task InitializeAsync()
        {
            _organization = await CreateOrganizationAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Returns_One_Row_Per_Supplier_With_Contracts_At_Highest_Criticality()
        {
            var organizationUuid = _organization.Uuid;
            var suffix = Guid.NewGuid().ToString("N");
            var supplierSharedName = $"supplier-{suffix}";
            var before1 = DateTime.UtcNow;
            var supplierA = await CreateOrganizationAsync(supplierSharedName, cvr: CreateCvr());
            var supplierB = await CreateOrganizationAsync(supplierSharedName, cvr: CreateCvr());

            var criticalities = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.CriticalityTypes, organizationUuid, 25, 0))
                .Take(3)
                .ToList();
            Assert.True(criticalities.Count >= 3);

            var low = criticalities[0];
            var medium = criticalities[1];
            var high = criticalities[2];

            SetCriticalityPriority(low.Uuid, 1);
            SetCriticalityPriority(medium.Uuid, 2);
            SetCriticalityPriority(high.Uuid, 3);

            var lowContractName = $"low-{suffix}";
            var mediumContractName = $"medium-{suffix}";
            var highContractName1 = $"high-a-{suffix}";
            var highContractName2 = $"high-b-{suffix}";
            var otherSupplierContractName = $"other-medium-{suffix}";
            var withoutSupplierName = $"without-supplier-{suffix}";

            await CreateContractWithSupplierAndCriticality(organizationUuid, supplierA.Uuid, low.Uuid, lowContractName);
            await CreateContractWithSupplierAndCriticality(organizationUuid, supplierA.Uuid, medium.Uuid, mediumContractName);
            await CreateContractWithSupplierAndCriticality(organizationUuid, supplierA.Uuid, high.Uuid, highContractName1);
            await CreateContractWithSupplierAndCriticality(organizationUuid, supplierA.Uuid, high.Uuid, highContractName2);
            await CreateContractWithSupplierAndCriticality(organizationUuid, supplierB.Uuid, medium.Uuid, otherSupplierContractName);
            await CreateItContractAsync(organizationUuid, withoutSupplierName);

            await ReadModelTestTools.WaitForReadModelQueueDepletion(before1);

            var queryResult = (await ItContractV2Helper.QuerySupplierOverviewReadModel(organizationUuid, odataOrderBy: "SupplierName asc")).ToList();

            Assert.Equal(2, queryResult.Count);

            var supplierARow = Assert.Single(queryResult, x => x.SupplierUuid == supplierA.Uuid);
            Assert.Equal(supplierSharedName, supplierARow.SupplierName);
            Assert.Equal(high.Name, supplierARow.HighestCriticalityName);
            Assert.Equal(3, supplierARow.HighestCriticalityRank);
            Assert.Contains(highContractName1, supplierARow.ContractsAtHighestCriticalityCsv);
            Assert.Contains(highContractName2, supplierARow.ContractsAtHighestCriticalityCsv);
            Assert.DoesNotContain(lowContractName, supplierARow.ContractsAtHighestCriticalityCsv);
            Assert.DoesNotContain(mediumContractName, supplierARow.ContractsAtHighestCriticalityCsv);
            Assert.Equal(2, supplierARow.ContractsAtHighestCriticality.Count);
            Assert.Contains(supplierARow.ContractsAtHighestCriticality, x => x.ContractName == highContractName1);
            Assert.Contains(supplierARow.ContractsAtHighestCriticality, x => x.ContractName == highContractName2);

            var supplierBRow = Assert.Single(queryResult,x => x.SupplierUuid == supplierB.Uuid);
            Assert.Equal(supplierSharedName, supplierBRow.SupplierName);
            Assert.Equal(otherSupplierContractName, supplierBRow.ContractsAtHighestCriticality.Single().ContractName);
            Assert.Equal(medium.Name, supplierBRow.HighestCriticalityName);
            Assert.Equal(2, supplierBRow.HighestCriticalityRank);
            var before2 = DateTime.UtcNow;

            SetCriticalityPriority(low.Uuid, 1);
            var mediumId = SetCriticalityPriority(medium.Uuid, 5);
            SetCriticalityPriority(high.Uuid, 3);
            ScheduleSupplierOverviewCriticalityUpdate(mediumId);
            await ReadModelTestTools.WaitForReadModelQueueDepletion(before2);

            var updatedResult = (await ItContractV2Helper.QuerySupplierOverviewReadModel(organizationUuid)).ToList();
            var updatedSupplierARow = Assert.Single(updatedResult, x => x.SupplierUuid == supplierA.Uuid);

            Assert.Equal(medium.Name, updatedSupplierARow.HighestCriticalityName);
            Assert.Equal(5, updatedSupplierARow.HighestCriticalityRank);
            Assert.Single(updatedSupplierARow.ContractsAtHighestCriticality);
            Assert.Equal(mediumContractName, updatedSupplierARow.ContractsAtHighestCriticality.Single().ContractName);
        }

        [Fact]
        public async Task Supports_Filter_Sort_And_Paging_On_Supplier_Overview()
        {
            var organizationUuid = _organization.Uuid;
            var suffix = Guid.NewGuid().ToString("N");

            var before = DateTime.UtcNow;
            var supplier1 = await CreateOrganizationAsync($"alpha-{suffix}", cvr: CreateCvr());
            var supplier2 = await CreateOrganizationAsync($"beta-{suffix}", cvr: CreateCvr());
            var supplier3 = await CreateOrganizationAsync($"gamma-{suffix}", cvr: CreateCvr());

            var criticality = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.CriticalityTypes, organizationUuid, 25, 0))
                .First();
            SetCriticalityPriority(criticality.Uuid, 10);

            var contractName1 = $"contract-filter-1-{suffix}";
            var contractName2 = $"contract-filter-2-{suffix}";
            var contractName3 = $"contract-other-{suffix}";

            await CreateContractWithSupplierAndCriticality(organizationUuid, supplier1.Uuid, criticality.Uuid, contractName1);
            await CreateContractWithSupplierAndCriticality(organizationUuid, supplier2.Uuid, criticality.Uuid, contractName2);
            await CreateContractWithSupplierAndCriticality(organizationUuid, supplier3.Uuid, criticality.Uuid, contractName3);

            await ReadModelTestTools.WaitForReadModelQueueDepletion(before);

            var filtered = (await ItContractV2Helper.QuerySupplierOverviewReadModel(
                organizationUuid,
                odataFilter: $"contains(ContractsAtHighestCriticalityCsv,'filter')",
                odataOrderBy: "SupplierName asc",
                top: 1,
                skip: 1)).ToList();

            var pagedRow = Assert.Single(filtered);
            Assert.Equal(supplier2.Uuid, pagedRow.SupplierUuid);
            Assert.Equal(contractName2, pagedRow.ContractsAtHighestCriticality.Single().ContractName);
        }

        [Fact]
        public async Task Includes_Supplier_Foreign_Cvr_For_Fallback()
        {
            var organizationUuid = _organization.Uuid;
            var suffix = Guid.NewGuid().ToString("N");
            var supplier = await CreateOrganizationAsync($"foreign-cvr-{suffix}", cvr: CreateCvr());
            var criticality = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.CriticalityTypes, organizationUuid, 25, 0))
                .First();
            var before = DateTime.UtcNow;

            var supplierId = SetSupplierForeignCvrAndClearCvr(supplier.Uuid, $"F-{suffix}");
            ScheduleSupplierOverviewOrganizationUpdate(supplierId);

            await CreateContractWithSupplierAndCriticality(organizationUuid, supplier.Uuid, criticality.Uuid, $"contract-{suffix}");
            await ReadModelTestTools.WaitForReadModelQueueDepletion(before);

            var queryResult = (await ItContractV2Helper.QuerySupplierOverviewReadModel(organizationUuid)).ToList();
            var supplierRow = Assert.Single(queryResult,x => x.SupplierUuid == supplier.Uuid);

            Assert.Null(supplierRow.SupplierCvr);
            Assert.Equal($"F-{suffix}", supplierRow.SupplierForeignCvr);
        }

        private async Task CreateContractWithSupplierAndCriticality(
            Guid organizationUuid,
            Guid supplierUuid,
            Guid criticalityUuid,
            string contractName)
        {
            var token = await GetGlobalToken();
            var contract = await CreateItContractAsync(organizationUuid, contractName);

            await ItContractV2Helper.SendPatchContractSupplierAsync(token, contract.Uuid,
                new ContractSupplierDataWriteRequestDTO
                {
                    OrganizationUuid = supplierUuid
                });

            await ItContractV2Helper.SendPatchContractGeneralDataAsync(token, contract.Uuid,
                new ContractGeneralDataWriteRequestDTO
                {
                    CriticalityUuid = criticalityUuid
                });
        }

        private static int SetCriticalityPriority(Guid criticalityUuid, int priority)
        {
            var criticalityId = 0;
            DatabaseAccess.MutateEntitySet<CriticalityType>(repository =>
            {
                var option = repository.AsQueryable().First(x => x.Uuid == criticalityUuid);
                option.Priority = priority;
                criticalityId = option.Id;
            });
            return criticalityId;
        }

        private static void ScheduleSupplierOverviewCriticalityUpdate(int criticalityId)
        {
            DatabaseAccess.MutateEntitySet<PendingReadModelUpdate>(repository =>
            {
                repository.Insert(PendingReadModelUpdate.Create(
                    criticalityId,
                    PendingReadModelUpdateSourceCategory.ItContract_SupplierOverview_CriticalityType));
            });
        }

        private static int SetSupplierForeignCvrAndClearCvr(Guid supplierUuid, string foreignCvr)
        {
            var supplierId = 0;
            DatabaseAccess.MutateEntitySet<Organization>(repository =>
            {
                var supplier = repository.AsQueryable().First(x => x.Uuid == supplierUuid);
                supplier.Cvr = null;
                supplier.ForeignCvr = foreignCvr;
                supplierId = supplier.Id;
            });
            return supplierId;
        }

        private static void ScheduleSupplierOverviewOrganizationUpdate(int supplierId)
        {
            DatabaseAccess.MutateEntitySet<PendingReadModelUpdate>(repository =>
            {
                repository.Insert(PendingReadModelUpdate.Create(
                    supplierId,
                    PendingReadModelUpdateSourceCategory.ItContract_SupplierOverview_Organization));
            });
        }
    }
}
