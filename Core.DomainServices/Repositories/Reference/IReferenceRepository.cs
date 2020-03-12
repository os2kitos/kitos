﻿using Core.DomainModel;
using Core.DomainModel.References;
using Core.DomainModel.Result;

namespace Core.DomainServices.Repositories.Reference
{
    public interface IReferenceRepository
    {
        Maybe<IEntityWithExternalReferences> GetRootEntity(int id, ReferenceRootType rootType);
        void SaveRootEntity(IEntityWithExternalReferences root);
        void Delete(ExternalReference reference);
    }
}
