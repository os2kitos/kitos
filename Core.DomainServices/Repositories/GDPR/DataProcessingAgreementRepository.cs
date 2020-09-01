using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Events;
using Core.DomainServices.Extensions;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.GDPR
{
    public class DataProcessingAgreementRepository : IDataProcessingAgreementRepository
    {
        private readonly IGenericRepository<DataProcessingAgreement> _repository;
        private readonly IDomainEvents _domainEvents;

        public DataProcessingAgreementRepository(IGenericRepository<DataProcessingAgreement> repository, IDomainEvents domainEvents)
        {
            _repository = repository;
            _domainEvents = domainEvents;
        }


        public DataProcessingAgreement Add(DataProcessingAgreement newAgreement)
        {
            var dataProcessingAgreement = _repository.Insert(newAgreement);
            _repository.Save();
            _domainEvents.Raise(new DataProcessingAgreementChanged(dataProcessingAgreement, DataProcessingAgreementChanged.ChangeType.Created));
            return dataProcessingAgreement;
        }

        public void DeleteById(int id)
        {
            var dataProcessingAgreement = _repository.GetByKey(id);
            if (dataProcessingAgreement != null)
            {
                _domainEvents.Raise(new DataProcessingAgreementChanged(dataProcessingAgreement, DataProcessingAgreementChanged.ChangeType.Deleted));
                _repository.DeleteByKeyWithReferencePreload(id);
                _repository.Save();
            }
        }

        public void Update(DataProcessingAgreement dataProcessingAgreement)
        {
            _domainEvents.Raise(new DataProcessingAgreementChanged(dataProcessingAgreement, DataProcessingAgreementChanged.ChangeType.Updated));
            _repository.Save();
        }

        public Maybe<DataProcessingAgreement> GetById(int id)
        {
            return _repository.GetByKey(id);
        }

        public IQueryable<DataProcessingAgreement> Search(int organizationId, Maybe<string> exactName)
        {
            return
                _repository.AsQueryable().ByOrganizationId(organizationId)
                    .Transform(previousQuery => exactName.Select(previousQuery.ByNameExact).GetValueOrFallback(previousQuery));
        }
    }
}
