using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

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
    public class TreeNode<T> : ITreeNode<T>, IDisposable
    {
        public class TreeNodeEventArgs : EventArgs
        {
            public IEnumerable<TreeNode<T>> Nodes { get; private set; }

            public TreeNodeEventArgs(IEnumerable<TreeNode<T>> nodes)
            {
                if (null == nodes) nodes = Enumerable.Empty<TreeNode<T>>();
                this.Nodes = nodes;
            }
        }

        private readonly ObservableCollection<TreeNode<T>> ObservableChildren;
        private bool IsDisposed;

        /// <summary>
        /// Gets the value of the current tree node.
        /// </summary>
        /// <value>The value of the current tree node.</value>
        public T Value { get; private set; }

        /// <summary>
        /// Gets the child nodes of the current tree node.
        /// </summary>        
        public IList<TreeNode<T>> Children => this.ObservableChildren;

        /// <summary>
        /// Gets or sets a callback that is called when children are added to this TreeNode<T>.
        /// </summary>
        public Action<TreeNodeEventArgs> OnChildrenAdded { get; set; }

        /// <summary>
        /// Gets or sets a callback that is called when children are removed from this TreeNode<T>.
        /// </summary>
        public Action<TreeNodeEventArgs> OnChildrenRemoved { get; set; }

        /// <summary>
        /// Creates a new instance of a TreeNode class.
        /// </summary>
        public TreeNode()
        {
            this.ObservableChildren = new ObservableCollection<TreeNode<T>>();
            this.ObservableChildren.CollectionChanged += this.OnChildrenCollectionChanged;
        }

        private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (NotifyCollectionChangedAction.Add == e.Action && null != this.OnChildrenAdded)
            {
                TreeNodeEventArgs treeNodeEventArgs = new TreeNodeEventArgs(e.NewItems.OfType<TreeNode<T>>());
                this.OnChildrenAdded.Invoke(treeNodeEventArgs);
            }
            else if (NotifyCollectionChangedAction.Remove == e.Action && null != this.OnChildrenRemoved)
            {
                TreeNodeEventArgs treeNodeEventArgs = new TreeNodeEventArgs(e.OldItems.OfType<TreeNode<T>>());
                this.OnChildrenRemoved.Invoke(treeNodeEventArgs);
            }
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

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.ObservableChildren.CollectionChanged -= this.OnChildrenCollectionChanged;
                    this.OnChildrenAdded = null;
                    this.OnChildrenRemoved = null;
                }

                this.IsDisposed = true;
            }
            else throw new ObjectDisposedException(nameof(TreeNode<T>));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
