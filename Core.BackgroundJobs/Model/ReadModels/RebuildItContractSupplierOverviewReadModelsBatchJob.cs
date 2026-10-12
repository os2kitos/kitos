using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.Contract;
using Infrastructure.Services.DataAccess;
using Serilog;
using OrganizationEntity = Core.DomainModel.Organization.Organization;

namespace Core.BackgroundJobs.Model.ReadModels
{
    public class RebuildItContractSupplierOverviewReadModelsBatchJob(
        ILogger logger,
        IPendingReadModelUpdateRepository pendingReadModelUpdateRepository,
        IItContractSupplierOverviewReadModelRepository supplierOverviewReadModelRepository,
        IItContractRepository itContractRepository,
        IGenericRepository<ItContractSupplierOverviewReadModel> lowLevelSupplierOverviewReadModelRepository,
        IGenericRepository<ItContractSupplierOverviewAtCriticalityContractReadModel>
            lowLevelSupplierContractRowRepository,
        IGenericRepository<PendingReadModelUpdate> lowLevelPendingReadModelUpdateRepository,
        IGenericRepository<CriticalityType> criticalityTypeRepository,
        IGenericRepository<OrganizationEntity> organizationRepository,
        ITransactionManager transactionManager,
        IDatabaseControl databaseControl)
        : IAsyncBackgroundJob
    {
        private const int BatchSize = 25;
        public string Id => StandardJobIds.UpdateItContractSupplierOverviewReadModels;

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var completedUpdates = 0;
            try
            {
                var pendingUpdates = pendingReadModelUpdateRepository
                    .GetMany(PendingReadModelUpdateSourceCategory.ItContract_SupplierOverview, BatchSize)
                    .ToList();

                foreach (var pendingUpdate in pendingUpdates)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    using var transaction = transactionManager.Begin();
                    logger.Debug("Rebuilding supplier overview read model for contract source id {sourceId}", pendingUpdate.SourceId);

                    RebuildAffectedSuppliers(pendingUpdate.SourceId);

                    lowLevelPendingReadModelUpdateRepository.Delete(pendingUpdate);
                    databaseControl.SaveChanges();
                    transaction.Commit();
                    completedUpdates++;
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Error while rebuilding supplier overview read models");
                return Task.FromResult(Result<string, OperationError>.Failure(new OperationError("Error during rebuild", OperationFailure.UnknownError)));
            }

            return Task.FromResult(Result<string, OperationError>.Success($"Completed {completedUpdates} updates"));
        }

        private void RebuildAffectedSuppliers(int contractId)
        {
            var supplierKeys = supplierOverviewReadModelRepository
                .GetByContractId(contractId)
                .Select(x => new SupplierKey
                {
                    OrganizationId = x.OrganizationId,
                    SupplierId = x.SupplierId
                })
                .ToList();

            var source = itContractRepository.GetById(contractId);
            if (source?.SupplierId != null)
            {
                supplierKeys.Add(new SupplierKey
                {
                    OrganizationId = source.OrganizationId,
                    SupplierId = source.SupplierId.Value
                });
            }

            foreach (var supplierKey in supplierKeys.Distinct())
            {
                RebuildSupplierReadModel(supplierKey.OrganizationId, supplierKey.SupplierId);
            }
        }

        private void RebuildSupplierReadModel(int organizationId, int supplierId)
        {
            var supplier = organizationRepository.GetByKey(supplierId);
            var organization = organizationRepository.GetByKey(organizationId);
            if (organization == null) return;

            var existingReadModel = supplierOverviewReadModelRepository.GetByOrganizationAndSupplier(organizationId, supplierId);

            if (supplier == null)
            {
                DeleteReadModelIfPresent(existingReadModel);
                return;
            }

            var contractRows = (
                    from contract in itContractRepository.AsQueryable()
                    where contract.OrganizationId == organizationId && contract.SupplierId == supplierId
                    join criticality in criticalityTypeRepository.AsQueryable()
                        on contract.CriticalityId equals criticality.Id into criticalityJoin
                    from criticality in criticalityJoin.DefaultIfEmpty()
                    select new ContractRow
                    {
                        ContractId = contract.Id,
                        ContractUuid = contract.Uuid,
                        ContractName = contract.Name ?? string.Empty,
                        CriticalityUuid = criticality != null ? criticality.Uuid : null,
                        CriticalityName = criticality != null ? criticality.Name : null,
                        CriticalityRank = criticality != null ? criticality.Priority : null,
                        HasInternalSupplier = contract.HasInternalSupplier ?? false
                    })
                .ToList();

            if (!contractRows.Any())
            {
                DeleteReadModelIfPresent(existingReadModel);
                return;
            }

            var highestRank = contractRows.Where(x => x.CriticalityRank.HasValue).Max(x => x.CriticalityRank);
            var contractsAtHighestCriticality = contractRows
                .Where(x => x.CriticalityRank == highestRank || (x.CriticalityRank == null && highestRank == null))
                .OrderBy(x => x.ContractName)
                .ThenBy(x => x.ContractId)
                .ToList();

            var highestCriticality = contractsAtHighestCriticality
                .Where(x => x.CriticalityUuid.HasValue)
                .OrderBy(x => x.CriticalityName)
                .FirstOrDefault();

            var readModel = existingReadModel.GetValueOrFallback(new ItContractSupplierOverviewReadModel
            {
                OrganizationId = organizationId,
                SupplierId = supplierId
            });

            readModel.SupplierType = contractsAtHighestCriticality.Any(x => x.HasInternalSupplier) ? ItContractSupplierType.Internal : ItContractSupplierType.External;
            readModel.SupplierUuid = supplier.Uuid;
            readModel.SupplierName = supplier.Name;
            readModel.SupplierCvr = supplier.Cvr;
            readModel.SupplierForeignCvr = supplier.ForeignCvr;
            readModel.IsSupplierDisabled = supplier.Disabled;
            readModel.HighestCriticalityUuid = highestCriticality?.CriticalityUuid;
            readModel.HighestCriticalityName = highestCriticality?.CriticalityName;
            readModel.HighestCriticalityRank = highestRank;

            SyncContractsAtHighestCriticality(readModel, contractsAtHighestCriticality);
            readModel.ContractsAtHighestCriticalityCsv = string.Join(", ", readModel.ContractsAtHighestCriticality.OrderBy(x => x.ContractName).Select(x => x.ContractName));

            if (!existingReadModel.HasValue)
            {
                lowLevelSupplierOverviewReadModelRepository.Insert(readModel);
            }
        }

        private void SyncContractsAtHighestCriticality(
            ItContractSupplierOverviewReadModel readModel,
            IReadOnlyCollection<ContractRow> contractsAtHighestCriticality)
        {
            var incomingByContractId = contractsAtHighestCriticality.ToDictionary(x => x.ContractId);
            var existingRows = readModel.ContractsAtHighestCriticality.ToList();

            foreach (var existingRow in existingRows.Where(existing => incomingByContractId.ContainsKey(existing.ContractId) == false))
            {
                readModel.ContractsAtHighestCriticality.Remove(existingRow);
                lowLevelSupplierContractRowRepository.Delete(existingRow);
            }

            foreach (var incomingRow in contractsAtHighestCriticality)
            {
                var existingRow = readModel.ContractsAtHighestCriticality.FirstOrDefault(x => x.ContractId == incomingRow.ContractId);
                if (existingRow == null)
                {
                    existingRow = new ItContractSupplierOverviewAtCriticalityContractReadModel
                    {
                        Parent = readModel
                    };
                    readModel.ContractsAtHighestCriticality.Add(existingRow);
                }

                existingRow.ContractId = incomingRow.ContractId;
                existingRow.ContractUuid = incomingRow.ContractUuid;
                existingRow.ContractName = incomingRow.ContractName;
            }
        }

        private void DeleteReadModelIfPresent(Maybe<ItContractSupplierOverviewReadModel> readModel)
        {
            if (readModel.HasValue)
            {
                lowLevelSupplierOverviewReadModelRepository.DeleteWithReferencePreload(readModel.Value);
            }
        }

        private class ContractRow
        {
            public int ContractId { get; init; }
            public Guid ContractUuid { get; init; }
            public string ContractName { get; init; } = string.Empty;
            public Guid? CriticalityUuid { get; init; }
            public string? CriticalityName { get; init; } = string.Empty;
            public int? CriticalityRank { get; init; }
            public bool HasInternalSupplier { get; init; }
        }

        private class SupplierKey : IEquatable<SupplierKey>
        {
            public int OrganizationId { get; init; }
            public int SupplierId { get; init; }

            public bool Equals(SupplierKey? other)
            {
                if (other == null)
                {
                    return false;
                }

                return OrganizationId == other.OrganizationId && SupplierId == other.SupplierId;
            }

            public override bool Equals(object? obj)
            {
                return ReferenceEquals(this, obj) || (obj is SupplierKey other && Equals(other));
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(OrganizationId, SupplierId);
            }
        }
    }
}
