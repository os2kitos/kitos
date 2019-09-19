namespace Core.DomainModel.ItSystem
{
    public class ItSystemDataWorkerRelation : Entity, IContextAware, ISystemModule
    {
        public int ItSystemId { get; set; }
        public virtual ItSystem ItSystem { get; set; }

        public int DataWorkerId { get; set; }
        public virtual Organization.Organization DataWorker { get; set; }

        public bool IsInContext(int organizationId)
        {
            return ItSystem?.IsInContext(organizationId) == true 
                   || organizationId == DataWorkerId;
        }
    }
}
