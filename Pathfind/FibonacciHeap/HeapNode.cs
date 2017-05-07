using System;
using System.Collections.Generic;

namespace FibonacciHeap
{
    /// <summary>
    /// Tree data structure that stores priority of each node
    /// </summary>
    /// <typeparam name="TPriority">The priority type</typeparam>
    /// <typeparam name="TItem">The type to store</typeparam>
    public class HeapNode<TPriority, TItem> where TPriority : IComparable<TPriority>
    {
        /// <summary>
        /// Parent node in tree
        /// </summary>
        public HeapNode<TPriority, TItem> Parent { get; set; }

        /// <summary>
        /// Child nodes in tree
        /// </summary>
        public HashSet<HeapNode<TPriority, TItem>> Children { get; set; }

        /// <summary>
        /// Has this node had a child cut?
        /// </summary>
        public bool Marked { get; set; }

        /// <summary>
        /// Number of children of this node
        /// </summary>
        public int Rank { get { return Children.Count; } }

        /// <summary>
        /// Priority of node
        /// </summary>
        public TPriority Priority { get; internal set; }

        /// <summary>
        /// Object being stored
        /// </summary>
        public TItem Value { get; set; }

        /// <summary>
        /// A childless, parentless tree storing the given object and priority
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="value"></param>
        public HeapNode(TPriority priority, TItem value)
        {
            // No children, no parent
            Children = new HashSet<HeapNode<TPriority, TItem>>();
            Parent = null;

            Priority = priority;
            Value = value;
        }

        /// <summary>
        /// Adds the given tree as a child of this tree
        /// </summary>
        /// <param name="node">The tree to become a child of this tree</param>
        public void AddChild(HeapNode<TPriority, TItem> node)
        {
            Children.Add(node);
            node.Parent = this;
        }

        /// <summary>
        /// Removes the given child from this tree's list of children
        /// </summary>
        /// <param name="child">The child to remove</param>
        public void RemoveChild(HeapNode<TPriority, TItem> child)
        {
            Children.Remove(child);
            child.Parent = null;
        }

        /// <summary>
        /// Unparents this child from its parent tree
        /// </summary>
        public void RemoveParent()
        {
            Parent.Children.Remove(this);
            Parent = null;
        }
    }
}
