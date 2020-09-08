using System.Linq;
using Core.DomainModel.GDPR;
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
            Notify(dataProcessingAgreement, LifeCycleEventType.Created);
            return dataProcessingAgreement;
        }

        public bool DeleteById(int id)
        {
            var dataProcessingAgreement = _repository.GetByKey(id);
            if (dataProcessingAgreement != null)
            {
                Notify(dataProcessingAgreement, LifeCycleEventType.Deleted);
                _repository.DeleteWithReferencePreload(dataProcessingAgreement);
                _repository.Save();
                return true;
            }

            return false;
        }

        public void Update(DataProcessingAgreement dataProcessingAgreement)
        {
            Notify(dataProcessingAgreement, LifeCycleEventType.Updated);
            _repository.Save();
        }

        public Maybe<DataProcessingAgreement> GetById(int id)
        {
            return _repository.GetByKey(id);
        }

        public IQueryable<DataProcessingAgreement> Search(int organizationId, Maybe<string> exactName)
        {
            return
                _repository
                    .AsQueryable()
                    .ByOrganizationId(organizationId)
                    .Transform(previousQuery => exactName.Select(previousQuery.ByNameExact).GetValueOrFallback(previousQuery));
        }

        private void Notify(DataProcessingAgreement dataProcessingAgreement, LifeCycleEventType changeType)
        {
            _domainEvents.Raise(new EntityLifeCycleEvent<DataProcessingAgreement>(changeType, dataProcessingAgreement));
        }
    }
}
