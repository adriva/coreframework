using System;
using System.Collections.Generic;

namespace Adriva.Common.Core
{
    /// <summary>
    /// Provides an implementation of IEqualityComparer&lt;T&gt; that uses the predicates for comparison operations.
    /// </summary>
    /// <typeparam name="T">The type of items that will be compared.</typeparam>
    public sealed class GenericEqualityComparer<T> : EqualityComparer<T> where T : class
    {

        private Func<T, T, bool> predicate;
        private Func<T, int> action;

        /// <summary>
        /// Initializes a new instance of GenericEqualityComparer&lt;T&gt; class.
        /// </summary>
        /// <param name="equalityPredicate">The predicate that will be used to check if given two items are equal.</param>
        /// <param name="hashAction">The predicate that will be used to generate a hash value for a given item.</param>
        public GenericEqualityComparer(Func<T, T, bool> equalityPredicate, Func<T, int> hashAction)
        {
            this.predicate = equalityPredicate;
            this.action = hashAction;
        }

        /// <summary>
        /// Checks if the given two items are equal using the predicate given.
        /// </summary>
        /// <param name="x">The first item that will be compared.</param>
        /// <param name="y">The second item that will be compared.</param>
        /// <returns>Tru if the two items are equal, otherwise False.</returns>
        public override bool Equals(T x, T y)
        {
            if (null == x && null == y) return true;
            if (null == x || null == y) return false;

            return this.predicate(x, y);
        }

        /// <summary>
        /// Returns the hash code for the given item using the predicate given.
        /// </summary>
        /// <param name="obj">The item that the hash code will be calculated for.</param>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode(T obj)
        {
            if (null == obj) return 0;
            return this.action(obj);
        }
    }
}
