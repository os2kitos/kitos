using System;
using System.Collections.Generic;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Qa.References;

namespace Core.DomainModel
{
    public enum Display
    {
        /// <summary>
        /// Determines which value to be shown in GUI
        /// </summary>
        Title = 0,
        ExternalId = 1,
        Url = 2
    }
    public class ExternalReference : Entity, IProjectModule, ISystemModule, IContractModule, IDataProcessingModule
    {

        public int? ItProject_Id { get; set; }
        public virtual ItProject.ItProject ItProject { get; set; }

        public int? Itcontract_Id { get; set; }
        public virtual ItContract.ItContract ItContract { get; set; }

        public int? ItSystemUsage_Id { get; set; }
        public virtual ItSystemUsage.ItSystemUsage ItSystemUsage { get; set; }

        public int? ItSystem_Id { get; set; }
        public virtual ItSystem.ItSystem ItSystem { get; set; }

        public int? DataProcessingAgreement_Id { get; set; }
        public virtual DataProcessingAgreement DataProcessingAgreement { get; set; }


        public string Title { get; set; }
        public string ExternalReferenceId { get; set; }
        public string URL { get; set; }
        public Display Display { get; set; }
        public DateTime Created { get; set; }

        public IEntityWithExternalReferences GetOwner()
        {
            return
                ItSystemUsage ??
                ItContract ??
                ItProject ??
                DataProcessingAgreement ??
                (IEntityWithExternalReferences)ItSystem;
        }

        public virtual ICollection<BrokenLinkInExternalReference> BrokenLinkReports { get; set; }
    }
}
