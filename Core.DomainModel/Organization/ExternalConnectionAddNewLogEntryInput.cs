using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.Organization
{
    public class ExternalConnectionAddNewLogEntryInput
    {
        public ExternalConnectionAddNewLogEntryInput(Guid uuid, string name, ConnectionUpdateOrganizationUnitChangeType type, string description)
        {
            Uuid = uuid;
            Name = name;
            Type = type;
            Description = description;
        }

        public Guid Uuid { get; }
        public string Name { get; }
        public ConnectionUpdateOrganizationUnitChangeType Type { get; }
        public string Description { get; }
    }
}
