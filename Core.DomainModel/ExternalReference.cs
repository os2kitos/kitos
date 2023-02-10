using System;
using System.Collections.Generic;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Qa.References;

namespace Core.DomainModel
{
    public class ExternalReference : Entity, ISystemModule, IContractModule, IDataProcessingModule, IHasUuid
    {
        public ExternalReference()
        {
            BrokenLinkReports = new List<BrokenLinkInExternalReference>();
            Uuid = Guid.NewGuid();
        }

        public Guid Uuid { get; set; }

        public int? Itcontract_Id { get; set; }
        public virtual ItContract.ItContract ItContract { get; set; }

        public int? ItSystemUsage_Id { get; set; }
        public virtual ItSystemUsage.ItSystemUsage ItSystemUsage { get; set; }

        public int? ItSystem_Id { get; set; }
        public virtual ItSystem.ItSystem ItSystem { get; set; }

        public int? DataProcessingRegistration_Id { get; set; }
        public virtual DataProcessingRegistration DataProcessingRegistration { get; set; }


        public string Title { get; set; }
        public string ExternalReferenceId { get; set; }
        public string URL { get; set; }
        public DateTime Created { get; set; }

        public IEntityWithExternalReferences GetOwner()
        {
            return
                ItSystemUsage ??
                ItContract ??
                DataProcessingRegistration ??
                (IEntityWithExternalReferences)ItSystem;
        }

        public bool IsMasterReference()
        {
            return GetOwner()?.Reference?.Uuid == Uuid;
        }

        public virtual ICollection<BrokenLinkInExternalReference> BrokenLinkReports { get; set; }
    }
}
