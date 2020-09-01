using System.Linq;
using Core.DomainModel.GDPR;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.GDPR
{
    public interface IDataProcessingAgreementReadModelRepository
    {
        DataProcessingAgreementReadModel Add(DataProcessingAgreementReadModel newModel);
        Maybe<DataProcessingAgreementReadModel> GetBySourceId(int sourceId);
        void Update(DataProcessingAgreementReadModel updatedModel);
        void DeleteBySourceId(int sourceId);
        IQueryable<DataProcessingAgreementReadModel> GetByOrganizationId(int organizationId);
    }
}
