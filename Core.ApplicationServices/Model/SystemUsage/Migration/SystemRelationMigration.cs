using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Model.SystemUsage.Migration
{
    public class SystemRelationMigration
    {
        public ItSystemUsage sourceSystem { get; }

        public ItSystemUsage targetSystem { get; }

        public ItInterface itInterface { get; }
        public bool itInterfaceToBeDeleted { get; }

        public ItContract itContract { get; }

        public SystemRelationMigration(
            ItSystemUsage sourceSystem,
            ItSystemUsage targetSystem, 
            ItInterface itInterface,
            bool itInterfaceToBeDeleted,
            ItContract itContract)
        {
            this.sourceSystem = sourceSystem;
            this.targetSystem = targetSystem;
            this.itInterface = itInterface;
            this.itContract = itContract;
            this.itInterfaceToBeDeleted = itInterfaceToBeDeleted;
        }
    }
}
