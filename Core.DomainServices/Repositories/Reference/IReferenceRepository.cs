using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.References;


namespace Core.DomainServices.Repositories.Reference
{
    public interface IReferenceRepository
    {
        Maybe<ExternalReference> Get(int referenceId);
        Maybe<ExternalReference> GetByUuid(Guid uuid);
        Maybe<IEntityWithExternalReferences> GetRootEntity(int id, ReferenceRootType rootType);
        IQueryable<ExternalReference> GetByRootType(ReferenceRootType rootType);
        void SaveRootEntity(IEntityWithExternalReferences root);
        void Delete(ExternalReference reference);
    }
}
