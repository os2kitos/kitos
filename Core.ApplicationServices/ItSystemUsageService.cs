using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using System.Linq;
using Core.DomainServices.Extensions;

namespace Core.ApplicationServices
{
    public class ItSystemUsageService : IItSystemUsageService
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;

        public ItSystemUsageService(
            IGenericRepository<ItSystemUsage> usageRepository)
        {
            _usageRepository = usageRepository;
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

        public ItSystemUsage GetByOrganizationAndSystemId(int organizationId, int systemId)
        {
            return _usageRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .FirstOrDefault(u => u.ItSystemId == systemId);
        }
    }
}
