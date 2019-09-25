using Core.DomainModel;

namespace Core.DomainServices.Repositories.Reference
{
    public class ReferenceRepository : IReferenceRepository
    {
        private readonly IGenericRepository<ExternalReference> _referenceRepository;

        public ReferenceRepository(IGenericRepository<ExternalReference> referenceRepository)
        {
            _referenceRepository = referenceRepository;
        }

        public void Delete(ExternalReference reference)
        {
            _referenceRepository.Delete(reference);
            _referenceRepository.Save();
        }
    }
}
