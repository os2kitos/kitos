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

        public virtual ICollection<KendoColumnConfiguration> Columns { get; set; }

        public void UpdateVersion()
        {
            Version = string.Join("", Columns.Where(x => !x.Hidden).OrderBy(x => x.Index).Select(x => x.PersistId)).GetHashCode().ToString();
        }
    }

    public enum OverviewType
    {
        ItSystemUsage = 0
    }

    public class KendoColumnConfiguration : IHasId
    {
        public int Id { get; set; }
        public string PersistId { get; set; }
        public int Index { get; set; }
        public bool Hidden { get; set; }

        public int KendoOrganizationalConfigurationId { get; set; }
        public virtual KendoOrganizationalConfiguration KendoOrganizationalConfiguration { get; set; }
    }
}
