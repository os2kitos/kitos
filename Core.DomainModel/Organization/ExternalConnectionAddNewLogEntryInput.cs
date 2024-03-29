﻿using System;

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
