using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;

namespace Core.DomainModel.Extensions
{
    public static class HierarchyExtensions
    {
        /// <summary>
        /// Based on the current root, returns a collection containing the current root as well as nodes in the entire subtree and the encestry
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<TEntity> FlattenCompleteHierarchy<TEntity>(this TEntity root)
            where TEntity : class, IHierarchy<TEntity>

        {
            return root.FlattenHierarchy().Concat(root.FlattenAncestry());
        }

        /// <summary>
        /// Based on the current root, returns a collection containing the current root as well as nodes in the entire subtree
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<TEntity> FlattenHierarchy<TEntity>(this TEntity root)
            where TEntity : class, IHierarchy<TEntity>

        {
            var unreached = new Queue<TEntity>();
            var reached = new List<TEntity>();

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

        public static IEnumerable<TEntity> FlattenAncestry<TEntity>(this TEntity currentEntity) where TEntity : class, IHierarchy<TEntity>
        {
            var currentRoot = currentEntity;

            //Continue until the root has been located
            while (true)
            {
                if (currentRoot.Parent == null)
                {
                    yield break;
                }

                yield return currentRoot.Parent;

                currentRoot = currentRoot.Parent;
            }
        }

        public static Maybe<TEntity> SearchAncestry<TEntity>(this TEntity currentEntity, Predicate<TEntity> condition) where TEntity : class, IHierarchy<TEntity>
        {
            var currentRoot = currentEntity;

            //Continue until the root has been located
            while (true)
            {
                if (currentRoot.Parent == null)
                {
                    return Maybe<TEntity>.None;
                }

                if (condition(currentRoot.Parent))
                {
                    return currentRoot.Parent;
                }

                currentRoot = currentRoot.Parent;
            }
        }
    }
}
