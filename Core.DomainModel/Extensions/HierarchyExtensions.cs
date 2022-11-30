using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;

namespace Core.DomainModel.Extensions
{
    public static class HierarchyExtensions
    {

        public static bool IsLeaf<TEntity>(this TEntity entity)
            where TEntity : class, IHierarchy<TEntity>
        {
            return entity.Children.Any() == false;
        }

        public static bool IsRoot<TEntity>(this TEntity entity)
            where TEntity : class, IHierarchy<TEntity>
        {
            return entity.Parent == null;
        }

        /// <summary>
        /// Based on the current root, returns a collection containing the current root as well as nodes in the entire subtree and the ancestry
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
                var currentParent = currentRoot.Parent;
                if (currentParent == null)
                {
                    yield break;
                }

                yield return currentParent;

                currentRoot = currentParent;
            }
        }

        public static Maybe<TEntity> SearchAncestry<TEntity>(this TEntity currentEntity, Predicate<TEntity> condition) where TEntity : class, IHierarchy<TEntity>
        {
            var currentRoot = currentEntity;

            //Continue until the root has been located
            while (true)
            {
                var currentParent = currentRoot.Parent;
                if (currentParent == null)
                {
                    return Maybe<TEntity>.None;
                }

                if (condition(currentParent))
                {
                    return currentParent;
                }

                currentRoot = currentParent;
            }
        }

        public static Maybe<TEntity> SearchSubTree<TEntity>(this TEntity root, Predicate<TEntity> condition) where TEntity : class, IHierarchy<TEntity>
        {
            var unreached = new Queue<TEntity>();

            unreached.Enqueue(root);

            //Process one level at the time
            while (unreached.Count > 0)
            {
                var orgUnit = unreached.Dequeue();
                if (condition(orgUnit))
                {
                    return orgUnit;
                }

                foreach (var child in orgUnit.Children)
                {
                    unreached.Enqueue(child);
                }
            }

            return Maybe<TEntity>.None;
        }
    }
}
