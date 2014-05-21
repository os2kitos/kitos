using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ItSystemUsage Add(int systemId, int orgId, User owner)
        {
            //Adding the usage
            var usage = new ItSystemUsage()
                {
                    ItSystemId = systemId,
                    OrganizationId = orgId,
                    ObjectOwner = owner
                };

            var system = _systemRepository.GetByKey(systemId);
            
            //Adding the interfaceExposures
            usage.InterfaceExposures = system.ExposedInterfaces.Select(theInterface => new InterfaceExposure()
            {
                Interface = theInterface
            }).ToList();

            _usageRepository.Insert(usage);
            _usageRepository.Save();

            //Adding the interfaceUsages
            foreach (var canUseInterface in system.CanUseInterfaces)
            {
                AddInterfaceUsage(usage, canUseInterface, true);
            }

            return usage;
        }

        public InterfaceUsage AddInterfaceUsage(int systemUsageId, ItSystem theInterface, bool isDefault = false)
        {
            var systemUsage = _usageRepository.GetByKey(systemUsageId);
            return AddInterfaceUsage(systemUsage, theInterface, isDefault);
        }

        public InterfaceUsage AddInterfaceUsage(ItSystemUsage systemUsage, ItSystem theInterface, bool isDefault = false)
        {
            //if the interface usage already exist, this is a no-op
            if (systemUsage.InterfaceUsages.Any(interfaceUsage => interfaceUsage.InterfaceId == theInterface.Id)) return;
            
            var usage = new InterfaceUsage()
                {
                    Interface = theInterface,
                    IsDefault = isDefault,
                    DataRowUsages = theInterface.DataRows.Select(dataRow => new DataRowUsage(){ DataRowId = dataRow.Id}).ToList()
                };

            _usageRepository.Update(systemUsage);
            _usageRepository.Save();

            return usage;
        }


    }
}
