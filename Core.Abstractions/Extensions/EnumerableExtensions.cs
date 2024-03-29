﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;

namespace Core.Abstractions.Extensions
{
    public static class EnumerableExtensions
    {
        public enum EnumerableDelta
        {
            Added,
            Removed
        }

        /// <summary>
        /// Computes the deltas between two collections of unique items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TIdentity"></typeparam>
        /// <param name="currentCollectionState">A collection o UNIQUE entities of <typeparam name="T"></typeparam></param>
        /// <param name="nextCollectionState">A collection o UNIQUE entities of <typeparam name="T"></typeparam></param>
        /// <param name="withIdentity">A function which provides an identity for an item of type <typeparam name="T"></typeparam></param>
        /// <returns></returns>
        public static IEnumerable<(EnumerableDelta delta, T item)> ComputeDelta<T, TIdentity>(this IEnumerable<T> currentCollectionState, IEnumerable<T> nextCollectionState, Func<T, TIdentity> withIdentity)
        {
            var next = nextCollectionState.ToList();
            var current = currentCollectionState.ToList();

            if (next.Select(withIdentity).Distinct().Count() != next.Count)
                throw new ArgumentException("Duplicates are not supported", nameof(nextCollectionState));

            if (current.Select(withIdentity).Distinct().Count() != current.Count)
                throw new ArgumentException("Duplicates are not supported", nameof(nextCollectionState));

            var newState = next.ToDictionary(withIdentity);
            var existingState = current.ToDictionary(withIdentity);

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
        /// <summary>
        /// Applies a new representation of <see cref="existingState"/> and performs additions/removals into from <see cref="newState"/> into <see cref="existingState"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TIdentity">This is used to extract the identity of the source type iow. the value derived from the instance which can be used to distinguish it from other instances of the same type.</typeparam>
        /// <param name="newState">A collection o UNIQUE entities of <typeparam name="T"></typeparam></param>
        /// <param name="existingState">A collection o UNIQUE entities of <typeparam name="T"></typeparam></param>
        /// <param name="withIdentity">A function which provides an identity for an item of type <typeparam name="T"></typeparam></param>
        public static void MirrorTo<T, TIdentity>(this IEnumerable<T> newState, ICollection<T> existingState, Func<T, TIdentity> withIdentity)
        {
            existingState
                .ComputeDelta(newState, withIdentity)
                .ApplyTo(existingState);
        }

        /// <summary>
        /// Equivalent to FirstOrDefault but for clients using the <see cref="Maybe{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Maybe<T> FirstOrNone<T>(this IEnumerable<T> src, Func<T, bool> predicate = null)
        {
            var condition = predicate ?? (_ => true);

            //This works better than firstOrDefault since this works for both reference and value types
            foreach (var item in src)
            {
                if (condition(item))
                {
                    return item;
                }
            }
            return Maybe<T>.None;
        }

        /// <summary>
        /// Returns a maybe of the head and tail of the input enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Maybe<(T head, IEnumerable<T> tail)> MatchHeadAndTail<T>(this IEnumerable<T> src)
        {
            if (src == null)
                return Maybe<(T head, IEnumerable<T> tail)>.None;

            var list = src.ToList();
            return list.Any() ? (list.First(), list.Skip(1)) : Maybe<(T head, IEnumerable<T> tail)>.None;
        }
    }
}
