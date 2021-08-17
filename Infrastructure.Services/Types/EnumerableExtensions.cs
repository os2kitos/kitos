using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Services.Types
{
    public static class EnumerableExtensions
    {
        public enum EnumerableDelta
        {
            Added,
            Removed
        }

        public static IEnumerable<(EnumerableDelta delta, T item)> ComputeDelta<T, TIdentity>(this IEnumerable<T> currentCollectionState, IEnumerable<T> nextCollectionState, Func<T, TIdentity> withIdentity)
        {
            var newState = nextCollectionState.ToDictionary(withIdentity);
            var existingState = currentCollectionState.ToDictionary(withIdentity);

            //Compute removals
            var removals = existingState
                .Values
                .Where(p => newState.ContainsKey(withIdentity(p)) == false)
                .ToList();

            foreach (var item in removals)
                yield return (EnumerableDelta.Removed, item);

            //Compute additions
            var additions = newState
                .Values
                .Where(p => existingState.ContainsKey(withIdentity(p)) == false)
                .ToList();

            foreach (var item in additions)
                yield return (EnumerableDelta.Added, item);
        }

        public static void ApplyTo<T>(this IEnumerable<(EnumerableDelta delta, T item)> deltas, ICollection<T> target)
        {
            foreach (var (delta, item) in deltas)
            {
                switch (delta)
                {
                    case EnumerableDelta.Added:
                        target.Add(item);
                        break;
                    case EnumerableDelta.Removed:
                        target.Remove(item);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static void MergeInto<T, TIdentity>(this IEnumerable<T> newState, ICollection<T> existingState, Func<T, TIdentity> withIdentity)
        {
            existingState
                .ComputeDelta(newState, withIdentity)
                .ApplyTo(existingState);
        }
    }
}
