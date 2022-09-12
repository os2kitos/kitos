using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract.Read;

namespace Core.ApplicationServices.Contract.ReadModels
{
    public interface IItContractOverviewReadModelsService
    {
        /// <summary>
        /// Query all contracts within the organization defined by <param name="organizationId"></param>
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Result<IQueryable<ItContractOverviewReadModel>, OperationError> GetByOrganizationId(int organizationId);
        /// <summary>
        /// Gets contracts where respsible org unit matches <param name="organizationUnitId"> or one of it's descendants</param>
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="organizationUnitId"></param>
        /// <returns>
        /// Contracts where responsibleOrganizationUnit equals <param name="organizationUnitId"> or an organization unit of it's sub tree</param>
        /// </returns>
        Result<IQueryable<ItContractOverviewReadModel>, OperationError> GetByOrganizationIdAndIdOrgOrganizationUnitSubTree(int organizationId, int organizationUnitId);
    }
}
