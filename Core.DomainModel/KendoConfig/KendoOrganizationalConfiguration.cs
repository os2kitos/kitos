using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel
{
    public class KendoOrganizationalConfiguration : Entity, IOwnedByOrganization
    {
        public KendoOrganizationalConfiguration()
        {
            VisibleColumns = new List<KendoColumnConfiguration>();
        }

        public OverviewType OverviewType { get; set; }
        public string Version { get; set; }
        public int OrganizationId { get; set; }
        public Organization.Organization Organization { get; set; }
        public virtual ICollection<KendoColumnConfiguration> VisibleColumns { get; set; }

        public void AddColumns(IEnumerable<KendoColumnConfiguration> newColumns)
        {
            newColumns.ToList().ForEach(x =>
            {
                x.KendoOrganizationalConfigurationId = Id;
                VisibleColumns.Add(x);
            });

            //TODO: JMO - det skal være en hash - ikke bare ebn kæmpe string
            Version = string.Join("", VisibleColumns.OrderBy(x => x.PersistId).Select(x => x.PersistId));
        }
    }
}
