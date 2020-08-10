using System.Linq;
using Core.DomainModel;
using Core.DomainModel.References;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.Reference
{
    public interface IReferenceRepository
    {
        Maybe<ExternalReference> Get(int referenceId);
        Maybe<IEntityWithExternalReferences> GetRootEntity(int id, ReferenceRootType rootType);
        IQueryable<ExternalReference> GetByRootType(ReferenceRootType rootType);
        void SaveRootEntity(IEntityWithExternalReferences root);
        void Delete(ExternalReference reference);
    }
}
