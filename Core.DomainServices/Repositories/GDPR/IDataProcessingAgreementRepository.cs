using System.Linq;
using Core.DomainModel.GDPR;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.GDPR
{
    public interface IDataProcessingAgreementRepository
    {
        DataProcessingAgreement Add(DataProcessingAgreement newAgreement);
        bool DeleteById(int id);
        void Update(DataProcessingAgreement dataProcessingAgreement);
        Maybe<DataProcessingAgreement> GetById(int id);
        IQueryable<DataProcessingAgreement> Search(int organizationId, Maybe<string> exactName);
    }
}
