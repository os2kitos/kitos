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

            //Adding the interfaceUsages
            usage.InterfaceUsages = system.CanUseInterfaces.Select(theInterface => new InterfaceUsage()
                {
                    Interface = theInterface,
                    IsLocked = true //this interface usage should NOT be removable!
                }).ToList();

            
            //Adding the interfaceExposures
            usage.InterfaceExposures = system.ExposedInterfaces.Select(theInterface => new InterfaceExposure()
            {
                Interface = theInterface
            }).ToList();

            _usageRepository.Insert(usage);
            _usageRepository.Save();

            return usage;
        }

        public void AddInterfaceUsage(ItSystemUsage systemUsage, int interfaceId)
        {
            //if the interface usage already exist, this is a no-op
            if (systemUsage.InterfaceUsages.Any(interfaceUsage => interfaceUsage.InterfaceId == interfaceId)) return;

            systemUsage.InterfaceUsages.Add(new InterfaceUsage()
                {
                    InterfaceId = interfaceId,
                    IsLocked = false //this interface usage should be removable!
                });

            _usageRepository.Update(systemUsage);
            _usageRepository.Save();
        }
    }
}
