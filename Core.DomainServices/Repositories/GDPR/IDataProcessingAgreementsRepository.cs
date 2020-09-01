using System.Linq;
using Core.DomainModel.GDPR;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.GDPR
{
    public interface IDataProcessingAgreementsRepository
    {
        DataProcessingAgreement Add(DataProcessingAgreement newAgreement);
        void DeleteById(int id);
        void Update(DataProcessingAgreement dataProcessingAgreement);
        Maybe<DataProcessingAgreement> GetById(int id);
        IQueryable<DataProcessingAgreement> Search(int organizationId, Maybe<string> exactName);
    }
}
