using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using NotImplementedException = System.NotImplementedException;

namespace Core.DomainServices.Repositories.Advice
{
    public class AdviceRepository : IAdviceRepository
    {
        private readonly IGenericRepository<DomainModel.Advice.Advice> _advicesRepository;
        private readonly IGenericRepository<ItContract> _contractRepository;
        private readonly IGenericRepository<ItInterface> _interfaceRepository;
        private readonly IGenericRepository<ItProject> _itProjectRepository;
        private readonly IGenericRepository<ItSystemUsage> _itSystemUsageRepository;

        public AdviceRepository(
            IGenericRepository<DomainModel.Advice.Advice> advicesRepository,
            IGenericRepository<ItContract> contractRepository,
            IGenericRepository<ItInterface> interfaceRepository,
            IGenericRepository<ItProject> itProjectRepository,
            IGenericRepository<ItSystemUsage> itSystemUsageRepository)
        {
            _advicesRepository = advicesRepository;
            _contractRepository = contractRepository;
            _interfaceRepository = interfaceRepository;
            _itProjectRepository = itProjectRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
        }

        public IQueryable<DomainModel.Advice.Advice> GetOrphans()
        {
            var contractAdvices = GetOrphans(_contractRepository.AsQueryable(), ObjectType.itContract);
            var interfaceAdvices = GetOrphans(_interfaceRepository.AsQueryable(), ObjectType.itInterface);
            var systemUsageAdvices = GetOrphans(_itSystemUsageRepository.AsQueryable(), ObjectType.itSystemUsage);
            var projectAdvices = GetOrphans(_itProjectRepository.AsQueryable(), ObjectType.itProject);

            return contractAdvices
                .Concat(interfaceAdvices)
                .Concat(systemUsageAdvices)
                .Concat(projectAdvices)
                .Distinct()
                .OrderBy(x => x.Id);
        }

        public void Delete(DomainModel.Advice.Advice advice)
        {
            _advicesRepository.DeleteWithReferencePreload(advice);
            _advicesRepository.Save();
        }

        private IQueryable<DomainModel.Advice.Advice> GetOrphans<TEntity>(IQueryable<TEntity> sourceCollection, ObjectType objectType) where TEntity : Entity
        {
            return
                _advicesRepository
                    .AsQueryable()
                    .Where(advice => advice.Type == objectType)
                    .Where(advice => sourceCollection.Any(entity => entity.Id == advice.RelationId) == false);
        }
    }
}
