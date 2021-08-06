using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel
{
    public class KendoOrganizationalConfiguration : Entity, IOwnedByOrganization
    {
        public KendoOrganizationalConfiguration()
        {
            Columns = new List<KendoColumnConfiguration>();
        }

        public OverviewType OverviewType { get; set; }
        public string Version { get; set; }
        public int OrganizationId { get; set; }
        public Organization.Organization Organization { get; set; }

        public virtual ICollection<KendoColumnConfiguration> Columns { get; set; } //TODO JMO: Rename til VisibleColmns

        /// <summary>
        /// TODO JMO: Det giver ikke mening at klienten skal kalde den her kode. Lav i stedet en metode der opdaterer "Columns" dvs. UpdateColumns(columns). Den opdaterer så propertien "Columns" og genberegner dernæst versionen
        /// </summary>
        public void UpdateVersion()
        {
            //TODO: JMO - det skal være en hash - ikke bare ebn kæmpe string
            Version = string.Join("", Columns.Where(x => !x.Hidden).OrderBy(x => x.Index).Select(x => x.PersistId));
        }
    }

    public enum OverviewType
    {
        ItSystemUsage = 0
    }

    //TODO: JMO lad os få enum og klassen her ud i egne filer
    public class KendoColumnConfiguration : IHasId
    {
        public int Id { get; set; }
        public string PersistId { get; set; }
        public int Index { get; set; }
        public bool Hidden { get; set; } //TODO: JMO - det er ikke relevant. Hvis ikke den "er her" så er den ikke synlig

        public int KendoOrganizationalConfigurationId { get; set; }
        public virtual KendoOrganizationalConfiguration KendoOrganizationalConfiguration { get; set; }
    }
}
