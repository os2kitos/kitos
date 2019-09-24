using Core.DomainModel;

namespace Core.DomainServices.Repositories.Reference
{
    public interface IReferenceRepository
    {
        void Delete(ExternalReference reference);
    }
}
