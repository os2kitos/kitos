using System;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.References;
using Core.DomainModel.Result;

namespace Core.DomainServices.Repositories.Reference
{
    public class ReferenceRepository : IReferenceRepository
    {
        private readonly IGenericRepository<ExternalReference> _referenceRepository;
        private readonly IGenericRepository<ItContract> _contractRepository;
        private readonly IGenericRepository<ItSystem> _systemRepository;
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly IGenericRepository<ItProject> _projectRepository;

        public ReferenceRepository(
            IGenericRepository<ExternalReference> referenceRepository, 
            IGenericRepository<ItContract> contractRepository, 
            IGenericRepository<ItSystem> systemRepository, 
            IGenericRepository<ItSystemUsage> systemUsageRepository, 
            IGenericRepository<ItProject> projectRepository)
        {
            _referenceRepository = referenceRepository;
            _contractRepository = contractRepository;
            _systemRepository = systemRepository;
            _systemUsageRepository = systemUsageRepository;
            _projectRepository = projectRepository;
        }

        public Maybe<IEntityWithExternalReferences> GetRootEntity(int id, ReferenceRootType rootType)
        {
            //TODO: Get repository ops, then GET
            //switch (rootType)
            //{
            //    case ReferenceRootType.System:
            //        return _systemRepository.GetByKey(id);
            //    case ReferenceRootType.SystemUsage:
            //        return _systemUsageRepository.GetByKey(id);
            //    case ReferenceRootType.Contract:
            //        return _contractRepository.GetByKey(id);
            //    case ReferenceRootType.Project:
            //        return _projectRepository.GetByKey(id);
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(rootType), rootType, null);
            //}
        }

        public void Save(IEntityWithExternalReferences root)
        {
            //TODO: Get repository ops, then SAVE
        }

        public void Delete(ExternalReference reference)
        {
            _referenceRepository.Delete(reference);
            _referenceRepository.Save();
        }
    }
}
