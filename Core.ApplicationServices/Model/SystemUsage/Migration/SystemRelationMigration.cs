using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Model.SystemUsage.Migration
{
    public class SystemRelationMigration
    {
        public SystemRelation Relation { get; }
        public bool ItInterfaceToBeDeleted { get; }

        public SystemRelationMigration(bool itInterfaceToBeDeleted, SystemRelation relation)
        {
            Relation = relation;
            ItInterfaceToBeDeleted = itInterfaceToBeDeleted;
        }
    }
}
