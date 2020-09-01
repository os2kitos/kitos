using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.GDPR
{
    public interface IDataProcessingAgreementReadService
    {
        Result<IQueryable<DataProcessingAgreementReadModel>, OperationError> GetByOrganizationId(int organizationId);
    }
}
