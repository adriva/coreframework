using System.Collections.Generic;

namespace Adriva.Common.Core.DataStructures
{
    /// <summary>
    /// Represents a hierarchical tree node with a value of type T.
    /// </summary>
    /// <typeparam name="T">The type of the value of the tree node.</typeparam>
    public interface ITreeNode<T>
    {
        T Value { get; }

        IList<TreeNode<T>> Children { get; }
    }

    /// <summary>
    /// A tree node with a value of type T.
    /// </summary>
    /// <typeparam name="T">The type of the value of the tree node.</typeparam>
    public class TreeNode<T> : ITreeNode<T>
    {
        /// <summary>
        /// Gets the value of the current tree node.
        /// </summary>
        /// <value>The value of the current tree node.</value>
        public T Value { get; private set; }

        /// <summary>
        /// Gets the child nodes of the current tree node.
        /// </summary>        
        public IList<TreeNode<T>> Children { get; private set; }

        /// <summary>
        /// Creates a new instance of a TreeNode class.
        /// </summary>
        public TreeNode()
        {
            this.Children = new List<TreeNode<T>>();
        }

        /// <summary>
        /// Creates a new instance of a TreeNode class with the specified value.
        /// </summary>
        /// <param name="value">The value of the new tree node object.</param>
        /// <returns>An instance of the TreeNode class.</returns>
        public TreeNode(T value) : this()
        {
            this.Value = value;
        }
    }

}
