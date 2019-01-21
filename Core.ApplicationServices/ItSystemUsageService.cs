using Core.DomainModel;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using System.Linq;

namespace Core.ApplicationServices
{
    public class ItSystemUsageService : IItSystemUsageService
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        private readonly IGenericRepository<ItInterfaceUsage> _interfaceUsageRepository;
        private readonly IGenericRepository<DataRowUsage> _dataRowUsageRepository;

        public ItSystemUsageService(
            IGenericRepository<ItSystemUsage> usageRepository,
            IGenericRepository<ItInterfaceUsage> interfaceUsageRepository,
            IGenericRepository<DataRowUsage> dataRowUsageRepository)
        {
            _usageRepository = usageRepository;
            _interfaceUsageRepository = interfaceUsageRepository;
            _dataRowUsageRepository = dataRowUsageRepository;
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

        ///// <summary>
        ///// Adds a new ItInterfaceUsage to an existing ItSystemUsage.
        ///// </summary>
        ///// <param name="usage">The ItSystemUsage</param>
        ///// <param name="theInterface">The new interface, which the ItInterfaceUsage should be generated from</param>
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
