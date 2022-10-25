using System.Collections.Generic;
using Core.DomainModel.Organization;

namespace Core.DomainModel.Extensions
{
    public static class ExternalOrganizationUnitExtensions
    {
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
