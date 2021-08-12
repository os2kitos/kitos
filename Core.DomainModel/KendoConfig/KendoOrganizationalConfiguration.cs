using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Core.DomainModel.KendoConfig
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


            Version = ComputeVersion();
        }

        public static KendoOrganizationalConfiguration CreateConfiguration(int orgId, OverviewType overviewType)
        {
            return new ()
            {
                OrganizationId = orgId,
                OverviewType = overviewType
            };
        }

        private string ComputeVersion()
        {
            var visibleColumnsAsString = string.Join("", VisibleColumns.OrderBy(x => x.PersistId).Select(x => x.PersistId));
            return Hash(visibleColumnsAsString);
        }

        private static string Hash(string input)
        {
            using var sha256Hash = SHA256.Create();
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
