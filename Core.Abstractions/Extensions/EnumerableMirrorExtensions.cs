using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;

namespace Core.Abstractions.Extensions
{
    public static class EnumerableMirrorExtensions
    {
        public enum MirrorAction
        {
            AddToDestination,
            RemoveFromDestination,
            MergeToDestination
        }

        public class MirrorActionContext<TSource, TDestination>
        {
            public Maybe<TSource> SourceValue { get; }
            public Maybe<TDestination> DestinationValue { get; }
            public MirrorAction Action { get; }

            public MirrorActionContext(Maybe<TSource> sourceValue, Maybe<TDestination> destinationValue, MirrorAction action)
            {
                SourceValue = sourceValue;
                DestinationValue = destinationValue;
                Action = action;
            }
        }

        /// <summary>
        /// Computes mirror actions between two collections of different types but with common identity
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source">Items which must be mirrored to the destination collection based on the id produced by <param name="computeSourceItemId"></param></param>
        /// <param name="destination"></param>
        /// <param name="computeSourceItemId">Id function for source items</param>
        /// <param name="computeDestinationItemId">Id function for destination items</param>
        public static IEnumerable<MirrorActionContext<TSource, TDestination>> ComputeMirrorActions<TSource, TDestination>(this IEnumerable<TSource> source, IEnumerable<TDestination> destination, Func<TSource, string> computeSourceItemId, Func<TDestination, string> computeDestinationItemId)
        {
            var sourceCollection = source.ToList();
            var destinationCollection = destination.ToList();

            if (sourceCollection.Select(computeSourceItemId).Distinct().Count() != sourceCollection.Count)
                throw new ArgumentException("Duplicates are not supported", nameof(source));

            if (destinationCollection.Select(computeDestinationItemId).Distinct().Count() != destinationCollection.Count)
                throw new ArgumentException("Duplicates are not supported", nameof(destination));

            var sourceState = sourceCollection.ToDictionary(computeSourceItemId);
            var destinationState = destinationCollection.ToDictionary(computeDestinationItemId);

            //Find intersections and items which must be removed based on evaluation of the destination collection
            foreach (var state in sourceState)
            {
                // Incoming item with same identity exists in the destination collection
                if (destinationState.TryGetValue(state.Key, out var existingValue))
                {
                    yield return new MirrorActionContext<TSource, TDestination>(state.Value, existingValue, MirrorAction.MergeToDestination);
                }
                //Item is new and must be added to the collection
                else
                {
                    yield return new MirrorActionContext<TSource, TDestination>(state.Value, Maybe<TDestination>.None, MirrorAction.AddToDestination);
                }

                //Remove evaluated item - all the leftovers must be removed since they are not part of the source collection
                destinationState.Remove(state.Key);
            }

            //Remaining items do not exist in the collection and must be removed from the destination collection
            foreach (var itemToBeRemoved in destinationState.Values)
            {
                yield return new MirrorActionContext<TSource, TDestination>(Maybe<TSource>.None, itemToBeRemoved, MirrorAction.RemoveFromDestination);
            }
        }
    }
}
