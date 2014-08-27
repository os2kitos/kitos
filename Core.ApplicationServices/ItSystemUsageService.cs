using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class ItSystemUsageService : IItSystemUsageService
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        private readonly IGenericRepository<ItSystem> _systemRepository;
        private readonly IGenericRepository<InterfaceExposure> _exposureRepository;
        private readonly IGenericRepository<InterfaceUsage> _interfaceUsageRepository;
        private readonly IGenericRepository<DataRowUsage> _dataRowUsageRepository;

        public ItSystemUsageService(
            IGenericRepository<ItSystemUsage> usageRepository, 
            IGenericRepository<ItSystem> systemRepository, 
            IGenericRepository<InterfaceExposure> exposureRepository, 
            IGenericRepository<InterfaceUsage> interfaceUsageRepository,
            IGenericRepository<DataRowUsage> dataRowUsageRepository)
        {
            _usageRepository = usageRepository;
            _systemRepository = systemRepository;
            _exposureRepository = exposureRepository;
            _interfaceUsageRepository = interfaceUsageRepository;
            _dataRowUsageRepository = dataRowUsageRepository;
        }

        public ItSystemUsage Add(int systemId, int orgId, User objectOwner)
        {
            // create the system usage
            var usage = _usageRepository.Create();
            usage.ItSystemId = systemId;
            usage.OrganizationId = orgId;
            usage.ObjectOwner = objectOwner;
            usage.LastChangedByUser = objectOwner;

            _usageRepository.Insert(usage);
            
            var system = _systemRepository.GetByKey(systemId);
            // add the interfaceUsages
            CreateAndInsertInterfaceUsage(system.CanUseInterfaces, objectOwner);
            
            // add the interfaceExposures
            CreateAndInsertExposuedInterfaceUsage(system.ExposedInterfaces, usage, objectOwner);

            _usageRepository.Save(); // abuse this as UoW
            return usage;
        }

        /// <summary>
        /// Adds a new InterfaceUsage to an existing ItSystemUsage.
        /// </summary>
        /// <param name="usage">The ItSystemUsage</param>
        /// <param name="theInterface">The new interface, which the InterfaceUsage should be generated from</param>
        public void AddInterfaceUsage(ItSystemUsage usage, ItSystem theInterface)
        {
            CreateAndInsertInterfaceUsage(theInterface.CanUseInterfaces, usage.ObjectOwner);
        }

        private void CreateAndInsertInterfaceUsage(IEnumerable<ItSystem> @interfaces, User objectOwner)
        {
            foreach (var @interface in @interfaces)
            {
                var interfaceUsage = _interfaceUsageRepository.Create();
                interfaceUsage.Interface = @interface;
                interfaceUsage.ObjectOwner = objectOwner;
                interfaceUsage.LastChangedByUser = objectOwner;

                _interfaceUsageRepository.Insert(interfaceUsage); // saveChanges is called in callee
                // add data row usages
                CreateAndInsertDataRowUsage(@interface.DataRows, objectOwner);
            }
        }

        private void CreateAndInsertDataRowUsage(IEnumerable<DataRow> dataRows, User objectOwner)
        {
            foreach (var dataRow in dataRows)
            {
                var dataRowUsage = new DataRowUsage()
                {
                    DataRowId = dataRow.Id,
                    ObjectOwner = objectOwner,
                    LastChangedByUser = objectOwner
                };

                _dataRowUsageRepository.Insert(dataRowUsage);  // saveChanges is called in callee
            }
        }

        private void CreateAndInsertExposuedInterfaceUsage(IEnumerable<ItSystem> exposedInterface, ItSystemUsage usage, User objectOwner)
        {
            foreach (var @interface in exposedInterface)
            {
                var interfaceUsage = _exposureRepository.Create();
                interfaceUsage.ItSystemUsage = usage;
                interfaceUsage.Interface = @interface;
                interfaceUsage.ObjectOwner = objectOwner;
                interfaceUsage.LastChangedByUser = objectOwner;

                _exposureRepository.Insert(interfaceUsage);  // saveChanges is called in callee
            }
        }
    }
}
