using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Organization;

namespace Core.DomainModel.Extensions
{
    public static class ExternalOrganizationUnitExtensions
    {
        /// <summary>
        /// Maps root external organization unit to parent tree
        /// </summary>
        /// <param name="root"></param>
        /// <param name="importedTreeByUuid"></param>
        /// <returns></returns>
        public static Dictionary<Guid, ExternalOrganizationUnit> ToParentMap(this ExternalOrganizationUnit root, Dictionary<Guid, ExternalOrganizationUnit> importedTreeByUuid)
        {
            var importedTreeToParent = importedTreeByUuid
                .Values
                .SelectMany(parent => parent.Children.Select(child => (child, parent)))
                .ToDictionary(x => x.child.Uuid, x => x.parent);

            importedTreeToParent.Add(root.Uuid, null); //Add the root as that will not be part of the collection
            return importedTreeToParent;
        }

        /// <summary>
        /// Maps root unit to flattened tree containing units children 
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static Dictionary<Guid, ExternalOrganizationUnit> ToLookupByUuid(this ExternalOrganizationUnit root)
        {
            var importedTreeByUuid = root
                .Flatten()
                .ToDictionary(x => x.Uuid);
            return importedTreeByUuid;
        }

        /// <summary>
        /// Based on the current root, returns a collection containing the current root as well as nodes in the entire subtree
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<ExternalOrganizationUnit> Flatten(this ExternalOrganizationUnit root)
        {
            var unreached = new Queue<ExternalOrganizationUnit>();
            var reached = new List<ExternalOrganizationUnit>();

            unreached.Enqueue(root);

            //Process one level at the time
            while (unreached.Count > 0)
            {
                var orgUnit = unreached.Dequeue();

                reached.Add(orgUnit);

                foreach (var child in orgUnit.Children)
                {
                    unreached.Enqueue(child);
                }
            }

            return reached;
        }
    }
}
