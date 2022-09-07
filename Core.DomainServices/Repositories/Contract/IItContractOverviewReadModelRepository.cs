using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract.Read;

namespace Core.DomainServices.Repositories.Contract
{
    public interface IItContractOverviewReadModelRepository
    {
        IQueryable<ItContractOverviewReadModel> GetByOrganizationId(int organizationId);
        IQueryable<ItContractOverviewReadModel> GetByOrganizationAndResponsibleOrganizationUnitIncludingSubTree(int organizationId, int responsibleOrganizationUnit);
        ItContractOverviewReadModel Add(ItContractOverviewReadModel model);
        void DeleteBySourceId(int sourceEntityId);
        Maybe<ItContractOverviewReadModel> GetBySourceId(int sourceId);
        IQueryable<ItContractOverviewReadModel> GetByOrganizationUnit(int orgUnitId);
        IQueryable<ItContractOverviewReadModel> GetByParentContract(int parentContractId);
        IQueryable<ItContractOverviewReadModel> GetByTerminationDeadlineType(int terminationDeadlineId);
        IQueryable<ItContractOverviewReadModel> GetByOptionExtendType(int optionExtendTypeId);
        IQueryable<ItContractOverviewReadModel> GetByItSystem(int itSystemId);
        IQueryable<ItContractOverviewReadModel> GetByItSystemUsage(int systemUsageId);
        IQueryable<ItContractOverviewReadModel> GetByDataProcessingRegistration(int dprId);
        IQueryable<ItContractOverviewReadModel> GetByUser(int userId);
        IQueryable<ItContractOverviewReadModel> GetByProcurementStrategyType(int procurementStrategyTypeId);
        IQueryable<ItContractOverviewReadModel> GetByPurchaseFormType(int purchaseFormTypeId);
        IQueryable<ItContractOverviewReadModel> GetByContractTemplateType(int contractTemplateTypeId);
        IQueryable<ItContractOverviewReadModel> GetByItContractType(int contractTypeId);
        IQueryable<ItContractOverviewReadModel> GetBySupplier(int organizationId);
        IQueryable<ItContractOverviewReadModel> GetByCriticalityType(int criticalityTypeId);
    }
}
