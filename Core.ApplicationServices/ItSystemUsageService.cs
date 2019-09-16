using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Repositories.SystemUsage;

namespace Core.ApplicationServices
{
    public class ItSystemUsageService : IItSystemUsageService
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        private readonly IItSystemUsageRepository _repository;
        private readonly IAuthorizationContext _authorizationContext;

        public ItSystemUsageService(
            IGenericRepository<ItSystemUsage> usageRepository,
            IItSystemUsageRepository repository,
            IAuthorizationContext authorizationContext)
        {
            _usageRepository = usageRepository;
            _repository = repository;
            _authorizationContext = authorizationContext;
        }

        public ItSystemUsage Add(ItSystemUsage ItSystemUsage, User objectOwner)
        {
            // create the system usage
            var usage = _usageRepository.Create();
            usage.ItSystemId = ItSystemUsage.ItSystemId;
            usage.OrganizationId = ItSystemUsage.OrganizationId;
            usage.ObjectOwner = objectOwner;
            usage.LastChangedByUser = objectOwner;
            usage.DataLevel = ItSystemUsage.DataLevel;
            usage.ContainsLegalInfo = ItSystemUsage.ContainsLegalInfo;
            usage.AssociatedDataWorkers = ItSystemUsage.AssociatedDataWorkers;
            _usageRepository.Insert(usage);
            _usageRepository.Save(); // abuse this as UoW
            return usage;
        }

        public void Delete(int id)
        {
            // http://stackoverflow.com/questions/15226312/entityframewok-how-to-configure-cascade-delete-to-nullify-foreign-keys
            // when children are loaded into memory the foreign key is correctly set to null on children when deleted
            var itSystemUsage = _usageRepository.Get(x => x.Id == id, null, $"{nameof(ItSystemUsage.ItInterfaceExhibitUsages)}, {nameof(ItSystemUsage.ItProjects)}, {nameof(ItSystemUsage.TaskRefs)}, {nameof(ItSystemUsage.Contracts)}, {nameof(ItSystemUsage.ItInterfaceUsages)}, {nameof(ItSystemUsage.UsedBy)}").FirstOrDefault();

            // delete it system usage
            _usageRepository.Delete(itSystemUsage);
            _usageRepository.Save();
        }

        public bool CanAddDataWorkerRelation(int usageId, int organizationId)
        {
            var systemUsage = _repository.GetSystemUsage(usageId);
            if (AllowDataWorkerCreation(systemUsage))
            {
                //Do not allow overlaps
                return systemUsage.AssociatedDataWorkers.Any(x => x.DataWorkerId == organizationId) == false;
            }

            return false;
        }

        private bool AllowDataWorkerCreation(IEntity systemUsage)
        {
            return systemUsage != null &&
                   _authorizationContext.AllowModify(systemUsage) &&
                   _authorizationContext.AllowCreate<ItSystemUsageDataWorkerRelation>();
        }
    }
}
