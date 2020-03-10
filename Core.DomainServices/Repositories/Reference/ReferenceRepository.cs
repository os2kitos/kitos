using System;
using System.Linq;
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
        private class ReferenceRootRepositoryOperations
        {
            private readonly Func<int, Maybe<IEntityWithExternalReferences>> _get;
            private readonly Action _save;

            public ReferenceRootRepositoryOperations(Func<int, Maybe<IEntityWithExternalReferences>> get, Action save)
            {
                _get = get;
                _save = save;
            }

            public Maybe<IEntityWithExternalReferences> Get(int id) => _get(id);
            public void Save() => _save();
        }

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
            return ResolveRepositoryOperations(rootType).Get(id);
        }

        public IQueryable<ExternalReference> GetByRootType(ReferenceRootType rootType)
        {
            var baseQuery = _referenceRepository.AsQueryable();
            switch (rootType)
            {
                case ReferenceRootType.System:
                    return baseQuery.Where(x => x.ItSystem_Id != null);
                case ReferenceRootType.SystemUsage:
                    return baseQuery.Where(x => x.ItSystemUsage_Id != null);
                case ReferenceRootType.Contract:
                    return baseQuery.Where(x => x.Itcontract_Id != null);
                case ReferenceRootType.Project:
                    return baseQuery.Where(x => x.ItProject_Id != null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(rootType), rootType, "Unknown reference root type");
            }
        }

        public void SaveRootEntity(IEntityWithExternalReferences root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            ResolveRepositoryOperations(root.GetRootType()).Save();
        }

        private ReferenceRootRepositoryOperations ResolveRepositoryOperations(ReferenceRootType rootType)
        {
            switch (rootType)
            {
                case ReferenceRootType.System:
                    return new ReferenceRootRepositoryOperations(innerId => _systemRepository.GetByKey(innerId), _systemRepository.Save);
                case ReferenceRootType.SystemUsage:
                    return new ReferenceRootRepositoryOperations(innerId => _systemUsageRepository.GetByKey(innerId), _systemUsageRepository.Save);
                case ReferenceRootType.Contract:
                    return new ReferenceRootRepositoryOperations(innerId => _contractRepository.GetByKey(innerId), _contractRepository.Save);
                case ReferenceRootType.Project:
                    return new ReferenceRootRepositoryOperations(innerId => _projectRepository.GetByKey(innerId), _projectRepository.Save);
                default:
                    throw new ArgumentOutOfRangeException(nameof(rootType), rootType, "Unknown reference root type");
            }
        }

        public void Delete(ExternalReference reference)
        {
            _referenceRepository.Delete(reference);
            _referenceRepository.Save();
        }
    }
}
