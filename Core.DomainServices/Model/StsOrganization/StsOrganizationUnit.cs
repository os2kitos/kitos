using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.DomainServices.Model.StsOrganization
{
    public class StsOrganizationUnit
    {
        public Guid Uuid { get; }
        public string Name { get; }
        public StsOrganizationUnit Parent { get; }
        public IEnumerable<StsOrganizationUnit> Children { get; }
        public bool IsRoot() => Parent == null;

        public StsOrganizationUnit(
            Guid uuid,
            string name,
            StsOrganizationUnit parent,
            IEnumerable<StsOrganizationUnit> children)
        {
            Uuid = uuid;
            Name = name;
            Parent = parent;
            Children = children?.ToList().AsReadOnly();
        }
    }
}
