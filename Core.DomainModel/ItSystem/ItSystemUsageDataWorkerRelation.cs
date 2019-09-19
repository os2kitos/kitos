namespace Core.DomainModel.ItSystem
{
    public class ItSystemUsageDataWorkerRelation : Entity, ISystemModule, IContextAware
    {
        public int ItSystemUsageId { get; set; }
        public virtual ItSystemUsage.ItSystemUsage ItSystemUsage { get; set; }

        public int DataWorkerId { get; set; }
        public virtual Organization.Organization DataWorker { get; set; }

        public bool IsInContext(int organizationId)
        {
            return ItSystemUsage?.IsInContext(organizationId) == true 
                   || DataWorkerId == organizationId;
        }
    }
}
