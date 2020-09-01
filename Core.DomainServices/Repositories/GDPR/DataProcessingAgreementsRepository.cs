using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.GDPR
{
    public class DataProcessingAgreementsRepository : IDataProcessingAgreementsRepository
    {
        private readonly IGenericRepository<DataProcessingAgreement> _repository;

        public DataProcessingAgreementsRepository(IGenericRepository<DataProcessingAgreement> repository)
        {
            _repository = repository;
        }


        public DataProcessingAgreement Add(DataProcessingAgreement newAgreement)
        {
            var dataProcessingAgreement = _repository.Insert(newAgreement);
            _repository.Save();
            //TODO: Publish changes
            return dataProcessingAgreement;
        }

        public void DeleteById(int id)
        {
            //TODO: Publish changes
            _repository.DeleteByKeyWithReferencePreload(id);
            _repository.Save();
        }

        public void Update(DataProcessingAgreement dataProcessingAgreement)
        {
            //TODO: Publish changes!
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
