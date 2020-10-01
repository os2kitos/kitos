using System;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.References;
using Infrastructure.Services.Types;

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
        private readonly IGenericRepository<DataProcessingRegistration> _dataProcessingRegistrationRepository;

        public ReferenceRepository(
            IGenericRepository<ExternalReference> referenceRepository,
            IGenericRepository<ItContract> contractRepository,
            IGenericRepository<ItSystem> systemRepository,
            IGenericRepository<ItSystemUsage> systemUsageRepository,
            IGenericRepository<ItProject> projectRepository,
            IGenericRepository<DataProcessingRegistration> dataProcessingRegistrationRepository)
        {
            _referenceRepository = referenceRepository;
            _contractRepository = contractRepository;
            _systemRepository = systemRepository;
            _systemUsageRepository = systemUsageRepository;
            _projectRepository = projectRepository;
            _dataProcessingRegistrationRepository = dataProcessingRegistrationRepository;
        }

        public Maybe<ExternalReference> Get(int referenceId)
        {
            return _referenceRepository.GetByKey(referenceId);
        }

        public Maybe<IEntityWithExternalReferences> GetRootEntity(int id, ReferenceRootType rootType)
        {
            return ResolveRepositoryOperations(rootType).Get(id);
        }

        public IQueryable<ExternalReference> GetByRootType(ReferenceRootType rootType)
        {
            var baseQuery = _referenceRepository.AsQueryable();
            return rootType switch
            {
                ReferenceRootType.System => baseQuery.Where(x => x.ItSystem_Id != null),
                ReferenceRootType.SystemUsage => baseQuery.Where(x => x.ItSystemUsage_Id != null),
                ReferenceRootType.Contract => baseQuery.Where(x => x.Itcontract_Id != null),
                ReferenceRootType.Project => baseQuery.Where(x => x.ItProject_Id != null),
                ReferenceRootType.DataProcessingRegistration => baseQuery.Where(x => x.DataProcessingRegistration_Id != null),
                _ => throw new ArgumentOutOfRangeException(nameof(rootType), rootType, "Unknown reference root type")
            };
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
            return rootType switch
            {
                ReferenceRootType.System =>
                    new ReferenceRootRepositoryOperations(innerId => _systemRepository.GetByKey(innerId), _systemRepository.Save),
                ReferenceRootType.SystemUsage =>
                    new ReferenceRootRepositoryOperations(innerId => _systemUsageRepository.GetByKey(innerId), _systemUsageRepository.Save),
                ReferenceRootType.Contract =>
                    new ReferenceRootRepositoryOperations(innerId => _contractRepository.GetByKey(innerId), _contractRepository.Save),
                ReferenceRootType.Project =>
                    new ReferenceRootRepositoryOperations(innerId => _projectRepository.GetByKey(innerId), _projectRepository.Save),
                ReferenceRootType.DataProcessingRegistration =>
                    new ReferenceRootRepositoryOperations(innerId => _dataProcessingRegistrationRepository.GetByKey(innerId), _dataProcessingRegistrationRepository.Save),
                _ => throw new ArgumentOutOfRangeException(nameof(rootType), rootType, "Unknown reference root type")
            };
        }

        public void Delete(ExternalReference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }
            _referenceRepository.Delete(reference);
            _referenceRepository.Save();
        }
    }
}
