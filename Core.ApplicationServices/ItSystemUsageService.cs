using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class ItSystemUsageService : IItSystemUsageService
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        private readonly IGenericRepository<InterfaceUsage> _interfaceUsageRepository;
        private readonly IGenericRepository<DataRowUsage> _dataRowUsageRepository;

        public ItSystemUsageService(
            IGenericRepository<ItSystemUsage> usageRepository, 
            IGenericRepository<InterfaceUsage> interfaceUsageRepository,
            IGenericRepository<DataRowUsage> dataRowUsageRepository)
        {
            _usageRepository = usageRepository;
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
            
            _usageRepository.Save(); // abuse this as UoW
            return usage;
        }

        ///// <summary>
        ///// Adds a new InterfaceUsage to an existing ItSystemUsage.
        ///// </summary>
        ///// <param name="usage">The ItSystemUsage</param>
        ///// <param name="theInterface">The new interface, which the InterfaceUsage should be generated from</param>
        //public void AddInterfaceUsage(ItSystemUsage usage, ItSystem theInterface)
        //{
        //    CreateAndInsertInterfaceUsage(theInterface.CanUseInterfaces, usage.ObjectOwner);
        //}

        //private void CreateAndInsertInterfaceUsage(IEnumerable<ItSystem> @interfaces, User objectOwner)
        //{
        //    foreach (var @interface in @interfaces)
        //    {
        //        var interfaceUsage = _interfaceUsageRepository.Create();
        //        interfaceUsage.ItInterfaceExhibits = @interface;
        //        interfaceUsage.ObjectOwner = objectOwner;
        //        interfaceUsage.LastChangedByUser = objectOwner;

        //        _interfaceUsageRepository.Insert(interfaceUsage); // saveChanges is called in callee
        //        // add data row usages
        //        CreateAndInsertDataRowUsage(@interface.DataRows, objectOwner);
        //    }
        //}

        //private void CreateAndInsertDataRowUsage(IEnumerable<DataRow> dataRows, User objectOwner)
        //{
        //    foreach (var dataRow in dataRows)
        //    {
        //        var dataRowUsage = new DataRowUsage()
        //        {
        //            DataRowId = dataRow.Id,
        //            ObjectOwner = objectOwner,
        //            LastChangedByUser = objectOwner
        //        };

        //        _dataRowUsageRepository.Insert(dataRowUsage); // saveChanges is called in callee
        //    }
        //}
    }
}
