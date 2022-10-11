using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;

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

        public ExternalOrganizationUnit Copy(Maybe<int> childLevelsToInclude)
        {
            var children = new List<ExternalOrganizationUnit>();
            var includeChildren = childLevelsToInclude
                .Select(levels=>levels > 0)
                .GetValueOrFallback(true); //IF no levels defined, include all

            if (includeChildren)
            {
                children = Children.Select(child =>
                {
                    var levelsToInclude = childLevelsToInclude.Select(levels => levels - 1);
                    return child.Copy(levelsToInclude);
                }).ToList();
            }

            return new ExternalOrganizationUnit(Uuid, Name, MetaData, children);
        }
    }
}
