using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.DomainServices.Model.StsOrganization
{
    public class StsOrganizationUnit
    {
        public Guid Uuid { get; }
        public string Name { get; }
        public string UserFacingKey { get; }
        public IEnumerable<StsOrganizationUnit> Children { get; }

        public StsOrganizationUnit(Guid uuid, string name, string userFacingKey, IEnumerable<StsOrganizationUnit> children)
        {
            Uuid = uuid;
            Name = name;
            UserFacingKey = userFacingKey;
            Children = children.ToList().AsReadOnly();
        }

        public StsOrganizationUnit Copy(uint? childLevelsToInclude = null)
        {
            var children = new List<StsOrganizationUnit>();
            var includeChildren = childLevelsToInclude is > 0 or null;
            if (includeChildren)
            {
                if (childLevelsToInclude.HasValue)
                {
                    childLevelsToInclude--;
                }
                children = Children.Select(child => child.Copy(childLevelsToInclude)).ToList();
            }

            return new StsOrganizationUnit(Uuid, Name, UserFacingKey, children);
        }
    }
}
