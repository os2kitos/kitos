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

        public ItSystemUsage Add(int systemId, int orgId, User objectOwner)
        {
            //Adding the usage
            var usage = new ItSystemUsage()
                {
                    ItSystemId = systemId,
                    OrganizationId = orgId,
                    ObjectOwner = objectOwner
                };

            var system = _systemRepository.GetByKey(systemId);

            //Adding the interfaceUsages
            usage.InterfaceUsages =
                system.CanUseInterfaces.Select(CreateInterfaceUsage).ToList();

            //Adding the interfaceExposures
            usage.InterfaceExposures = system.ExposedInterfaces.Select(theInterface => new InterfaceExposure()
            {
                Interface = theInterface
            }).ToList();

            _usageRepository.Insert(usage);
            _usageRepository.Save();

            return usage;
        }

        private InterfaceUsage CreateInterfaceUsage(ItSystem theInterface)
        {
            var usage = new InterfaceUsage()
                {
                    Interface = theInterface,
                    DataRowUsages = theInterface.DataRows.Select(dataRow => new DataRowUsage(){ DataRowId = dataRow.Id}).ToList()
                };
            
            return usage;
        }


    }
}
