using System;
using System.Collections.Generic;
using System.Linq;

namespace Adriva.Common.Core.DataStructures
{
    /// <summary>
    /// Provides extension methods to work with tree data structures.
    /// </summary>
    public static class TreeNodeExtensions
    {
        /// <summary>
        /// Flattens a given tree structure.
        /// </summary>
        /// <param name="node">The root node of the tree that will be flattened.</param>
        /// <typeparam name="T">Type of the values of the tree nodes.</typeparam>
        /// <returns>A LinkedList&lt;T&gt; that stores the TreeNode&lt;T&gt; classes in a linear order.</returns>
        public static LinkedList<T> Flatten<T>(this ITreeNode<T> node)
        {
            LinkedList<T> output = new LinkedList<T>();
            Queue<ITreeNode<T>> queue = new Queue<ITreeNode<T>>();
            queue.Enqueue(node);

            while (0 < queue.Count)
            {
                var treeNode = queue.Dequeue();
                output.AddLast(treeNode.Value);

                foreach (var childNode in treeNode.Children)
                {
                    queue.Enqueue(childNode);
                }
            }

            return output;
        }

        /// <summary>
        /// Creates a graph structure using the given flat sequence of items that has a parent to child relationship defined.
        /// </summary>
        /// <param name="items">Items that will be converted into a tree structure.</param>
        /// <param name="idResolver">A function that will be used to generate unique id of a tree node for an item.</param>
        /// <param name="parentIdResolver">A function that will be used to retrieve the parent id of a tree node for an item.</param>
        /// <typeparam name="TValue">The value type of the TreeNode.</typeparam>
        /// <returns>A sequence of TreeNode classes that represents the root nodes.</returns>
        public static IEnumerable<TreeNode<TValue>> CreateTree<TValue>(this IEnumerable<TValue> items, Func<TValue, long> idResolver, Func<TValue, long?> parentIdResolver)
        {
            if (null == items) return null;

            if (null == idResolver) throw new ArgumentNullException(nameof(idResolver));
            if (null == parentIdResolver) throw new ArgumentNullException(nameof(parentIdResolver));

            Dictionary<long, TreeNode<TValue>> itemsLookup = items.ToDictionary(idResolver, x => new TreeNode<TValue>(x));
            Dictionary<long, bool> processedLookup = new Dictionary<long, bool>();

            bool isNoop;

            do
            {
                isNoop = true;
                foreach (var nodePair in itemsLookup)
                {
                    long? parentId = parentIdResolver.Invoke(nodePair.Value.Value);

                    if (parentId.HasValue)
                    {
                        if (itemsLookup.ContainsKey(parentId.Value) && !processedLookup.ContainsKey(idResolver(nodePair.Value.Value)))
                        {
                            itemsLookup[parentId.Value].Children.Add(nodePair.Value);
                            processedLookup[idResolver(nodePair.Value.Value)] = true;
                            isNoop = false;
                        }
                    }
                }
            } while (!isNoop);

            var nonRootItems = itemsLookup.Where(x => parentIdResolver(x.Value.Value).HasValue).ToArray();

            foreach (var nonRootItem in nonRootItems)
            {
                itemsLookup.Remove(nonRootItem.Key);
            }

            processedLookup.Clear();
            return itemsLookup.Values;
        }

        /// <summary>
        /// Creates a graph structure using the given flat sequence of items that has a parent to child relationship defined.
        /// </summary>
        /// <param name="items">Items that will be converted into a tree structure.</param>
        /// <param name="idResolver">A function that will be used to generate unique id of a tree node for an item.</param>
        /// <param name="parentIdResolver">A function that will be used to retrieve the parent id of a tree node for an item.</param>
        /// <typeparam name="TValue">The value type of the TreeNode.</typeparam>
        /// <returns>A sequence of TreeNode classes that represents the root nodes.</returns>
        public static IEnumerable<TreeNode<TValue>> CreateTree<TValue>(this IEnumerable<TValue> items, Func<TValue, string> idResolver, Func<TValue, string> parentIdResolver)
        {
            if (null == items) return null;

            if (null == idResolver) throw new ArgumentNullException(nameof(idResolver));
            if (null == parentIdResolver) throw new ArgumentNullException(nameof(parentIdResolver));

            Dictionary<string, TreeNode<TValue>> itemsLookup = items.ToDictionary(idResolver, x => new TreeNode<TValue>(x));
            Dictionary<string, bool> processedLookup = new Dictionary<string, bool>();

            bool isNoop;

            do
            {
                isNoop = true;
                foreach (var nodePair in itemsLookup)
                {
                    string parentId = parentIdResolver.Invoke(nodePair.Value.Value);

                    if (!string.IsNullOrWhiteSpace(parentId))
                    {
                        if (itemsLookup.ContainsKey(parentId) && !processedLookup.ContainsKey(idResolver(nodePair.Value.Value)))
                        {
                            itemsLookup[parentId].Children.Add(nodePair.Value);
                            processedLookup[idResolver(nodePair.Value.Value)] = true;
                            isNoop = false;
                        }
                    }
                }
            } while (!isNoop);

            var nonRootItems = itemsLookup.Where(x => !string.IsNullOrWhiteSpace(parentIdResolver(x.Value.Value))).ToArray();

            foreach (var nonRootItem in nonRootItems)
            {
                itemsLookup.Remove(nonRootItem.Key);
            }

            processedLookup.Clear();
            return itemsLookup.Values;
        }
    }
}
