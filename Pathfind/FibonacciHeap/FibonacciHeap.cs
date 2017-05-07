using System;
using System.Collections.Generic;

namespace FibonacciHeap
{
    /// <summary>
    /// Min heap data structure that lazily defers consolidation of trees until deleteMin operation
    /// </summary>
    public class FibonacciHeap<TPriority, TItem> where TPriority : IComparable<TPriority>
    {
        /// <summary>
        /// All trees in the heap
        /// </summary>
        public LinkedList<HeapNode<TPriority, TItem>> Trees { get; private set; }

        /// <summary>
        /// Node with the smallest priority
        /// </summary>
        public HeapNode<TPriority, TItem> Minimum { get { return minimumTreesNode.Value; } }

        /// <summary>
        /// Number of items in the heap
        /// </summary>
        public int Count { get; private set; }

        private LinkedListNode<HeapNode<TPriority, TItem>> minimumTreesNode;

        /// <summary>
        /// An empty collection
        /// </summary>
        public FibonacciHeap()
        {
            Trees = new LinkedList<HeapNode<TPriority, TItem>>();
            Count = 0;
            minimumTreesNode = null;
        }

        /// <summary>
        /// Removes the item with the smallest priority from the heap and returns it. O(log n) operation
        /// </summary>
        /// <returns>The item with the smallest priority</returns>
        public TItem Pop()
        {
            TItem min = Minimum.Value;
            DeleteMin();
            return min;
        }

        /// <summary>
        /// Inserts the given item into the heap maintaining the heap order property. O(1) operation
        /// </summary>
        /// <param name="priority">The priority of the item</param>
        /// <param name="value">The object to store</param>
        /// <returns>The node that the object is stored in</returns>
        public HeapNode<TPriority, TItem> Insert(TPriority priority, TItem value)
        {
            // Add node as first item in root nodes
            HeapNode<TPriority, TItem> node = new HeapNode<TPriority, TItem>(priority, value);
            Trees.AddFirst(node);

            // Update minimum
            if (Count == 0 ||                                       // Node is first node inserted? Its minimum
                node.Priority.CompareTo(Minimum.Priority) < 0)      // Node is less than current minimum? Its minimum
                minimumTreesNode = Trees.First;

            Count++;
            return node;
        }

        /// <summary>
        /// Removes the minimum item from the heap. O(log n) operation
        /// </summary>
        public void DeleteMin()
        {
            if (Count == 0)
                throw new InvalidOperationException("Heap is empty, cannot remove minimum priority item");

            // Remove minimum element from list of trees
            RemoveMinimum();

            // Consolidate trees
            ConsolidateTrees();

            // Update new minimum value
            FindMinimum();
        }

        /// <summary>
        /// Decreases the priority of the given node. O(log n) operation
        /// </summary>
        /// <param name="node">The node to decrease the value of</param>
        /// <param name="priority">The new priority of the node</param>
        public void DecreasePriority(HeapNode<TPriority, TItem> node, TPriority priority)
        {
            // If node priority is already less than given priority
            if (node.Priority.CompareTo(priority) < 0)
                throw new ArgumentException("Priority is not less than current priority");

            // Decrease key of node
            node.Priority = priority;

            // If node is a root node, AND its priority is less than the current minimum
            if (node.Parent == null &&
                priority.CompareTo(Minimum.Priority) < 0)
                minimumTreesNode = Trees.Find(node);        // Set as minimum

            // Is heap order violated?
            if (node.Parent != null &&
                node.Priority.CompareTo(node.Parent.Priority) < 0)
            {
                // Cut from parent and move to root (remember ref to old parent). Node becomes unmarked in the process
                HeapNode<TPriority, TItem> parent = MoveToRoot(node);

                // Is new priority less than minimum?
                if (priority.CompareTo(Minimum.Priority) < 0)
                    minimumTreesNode = Trees.First;             // Node is first element in trees list

                // If parent is unmarked, mark it
                if (!parent.Marked)
                    parent.Marked = true;

                else    // Parent IS already marked
                {
                    // Cut parent, move to root, DO FOR ALL ANCESTORS
                    while (parent != null &&
                           parent.Marked)
                        parent = MoveToRoot(parent);    // Move to root, check its parent
                }
            }
        }

        /// <summary>
        /// Cuts the given node from its parent and moves it to the list of root trees. Returns the parent of the node, prior to it 
        /// being cut
        /// </summary>
        private HeapNode<TPriority, TItem> MoveToRoot(HeapNode<TPriority, TItem> node)
        {
            // Remeber ref to parent because child is about to be cut
            HeapNode<TPriority, TItem> parent = node.Parent;

            // Remove ref to parent and parent ref to child
            node.RemoveParent();
            Trees.AddFirst(node);
            node.Marked = false;

            // Return old parent of node
            return parent;
        }

        /// <summary>
        /// Removes the minimum priorirty node from the list of root trees. Adds children to the list of root trees
        /// </summary>
        private void RemoveMinimum()
        {
            // Add each child as a root tree, remove ref to parent
            foreach (HeapNode<TPriority, TItem> child in minimumTreesNode.Value.Children)
            {
                child.Parent = null;
                Trees.AddFirst(child);
            }

            // Remove minimum from list of trees
            Trees.Remove(minimumTreesNode);
            Count--;
        }

        /// <summary>
        /// Finds the new minimum value from amongst the root tree nodes
        /// </summary>
        private void FindMinimum()
        {
            // If there is atleast one node in heap
            if (Count > 0)
            {
                // minimum = first tree in list
                // walker through remaining nodes to see if there is a smaller one
                minimumTreesNode = Trees.First;

                // Walk through root element of each root tree
                LinkedListNode<HeapNode<TPriority, TItem>> walker = Trees.First.Next;
                while (walker != null)
                {
                    // Check if priority is less than current minimum, update minimum if so
                    if (walker.Value.Priority.CompareTo(minimumTreesNode.Value.Priority) < 0)
                        minimumTreesNode = walker;

                    // Keep iterating
                    walker = walker.Next;
                }
            }

            // No nodes in heap, min is null
            else
                minimumTreesNode = null;
        }

        /// <summary>
        /// Consolidates root trees that have the same rank, so that, no two root trees end with the same rank
        /// </summary>
        private void ConsolidateTrees()
        {
            // Need atleast 2 trees in order to consolidate
            if (Count > 1)
            {
                // Bucket index corresponds to the rank of the stored element
                Bucket<LinkedListNode<HeapNode<TPriority, TItem>>> rankBucket = new Bucket<LinkedListNode<HeapNode<TPriority, TItem>>>();

                // Iterate over each root tree
                var walker = Trees.First;
                while (walker != null)
                {
                    // Save ref to next root, because walker is going to be overwritten when there is a conflict
                    var next = walker.Next;

                    // Are there root trees with the same rank?
                    int rank = walker.Value.Rank;
                    while (rankBucket[rank] != null)
                    {

                        walker = Consolidate(walker, rankBucket[rank]);     // Consolidate trees with same rank (also get ref to merged tree)
                        rankBucket[rank] = null;                            // Remove ref to root tree that was consolidated
                        rank = walker.Value.Rank;                           // Update rank
                    }

                    // No conflicts, store in rank bucket
                    rankBucket[rank] = walker;

                    walker = next;
                }
            }
        }

        /// <summary>
        /// Merge trees into one tree, maintaining heap property (every child is larger than parent). Returns the merged tree's node in
        /// root trees list
        /// </summary>
        private LinkedListNode<HeapNode<TPriority, TItem>> Consolidate(LinkedListNode<HeapNode<TPriority, TItem>> node1,
                                                                       LinkedListNode<HeapNode<TPriority, TItem>> node2)
        {
            var root = node1;
            var child = node2;

            // Get node with least priority
            if (node2.Value.Priority.CompareTo(node1.Value.Priority) < 0)
            {
                root = node2;
                child = node1;
            }

            // Remove tree with greater priority, add as child of other tree
            Trees.Remove(child);
            root.Value.AddChild(child.Value);

            // Return combined tree
            return root;
        }
    }
}
