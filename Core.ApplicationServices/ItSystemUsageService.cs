using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class ItSystemUsageService : IItSystemUsageService
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        private readonly IGenericRepository<ItSystem> _systemRepository;

        public ItSystemUsageService(IGenericRepository<ItSystemUsage> usageRepository, IGenericRepository<ItSystem> systemRepository)
        {
            _usageRepository = usageRepository;
            _systemRepository = systemRepository;
        }

        public ItSystemUsage Add(int systemId, int orgId, User objectOwner)
        {
            //Adding the usage
            var usage = new ItSystemUsage()
                {
                    ItSystemId = systemId,
                    OrganizationId = orgId,
                    ObjectOwner = objectOwner,
                    LastChangedByUser = objectOwner
                };

            var system = _systemRepository.GetByKey(systemId);

            //Adding the interfaceUsages
            usage.InterfaceUsages = system.CanUseInterfaces.Select(itSys => CreateInterfaceUsage(itSys, objectOwner)).ToList();

            //Adding the interfaceExposures
            usage.InterfaceExposures = system.ExposedInterfaces.Select(theInterface => new InterfaceExposure()
            {
                Interface = theInterface,
                ObjectOwner = objectOwner,
                LastChangedByUser = objectOwner
            }).ToList();

            _usageRepository.Insert(usage);
            _usageRepository.Save();

            return usage;
        }

        private InterfaceUsage CreateInterfaceUsage(ItSystem theInterface, User objectOwner)
        {
            var dataRowUsages = theInterface.DataRows.Select(dataRow =>
                                                             new DataRowUsage()
                                                                 {
                                                                     DataRowId = dataRow.Id,
                                                                     ObjectOwner = objectOwner,
                                                                     LastChangedByUser = objectOwner
                                                                 }).ToList();

            var usage = new InterfaceUsage()
                {
                    Interface = theInterface,
                    DataRowUsages = dataRowUsages,
                    ObjectOwner = objectOwner,
                    LastChangedByUser = objectOwner
                };
            
            return usage;
        }


    }
}
