using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel.Organization
{
    /// <summary>
    /// Represents an external organization unit not part of the KITOS persisted data model
    /// </summary>
    public class ExternalOrganizationUnit
    {
        public Guid Uuid { get; }
        public string Name { get; }
        public IReadOnlyDictionary<string, string> MetaData { get; }
        public IEnumerable<ExternalOrganizationUnit> Children { get; }

        public ExternalOrganizationUnit(Guid uuid, string name, IReadOnlyDictionary<string, string> metaData, IEnumerable<ExternalOrganizationUnit> children)
        {
            Uuid = uuid;
            Name = name;
            MetaData = metaData ?? new Dictionary<string, string>();
            Children = children.ToList().AsReadOnly();
        }

        public ExternalOrganizationUnit Copy(int? childLevelsToInclude = null)
        {
            if (childLevelsToInclude is < 1)
            {
                throw new ArgumentException("Invalid sync depth");
            }
            var children = new List<ExternalOrganizationUnit>();
            var includeChildren = childLevelsToInclude is > 0 or null;
            if (includeChildren)
            {
                if (childLevelsToInclude.HasValue)
                {
                    childLevelsToInclude--;
                }
                children = Children.Select(child => child.Copy(childLevelsToInclude)).ToList();
            }

            return new ExternalOrganizationUnit(Uuid, Name, MetaData, children);
        }
    }
}
