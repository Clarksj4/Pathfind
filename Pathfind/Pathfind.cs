using System;
using System.Collections.Generic;
using System.Linq;
using FibonacciHeap;

namespace Pathfinding
{
    /// <summary>
    /// Utility methods for finding paths, areas, and ranges on a hex map
    /// </summary>
    public static class Pathfind
    {
        private static HashSet<IGraphNode> evaluated;
        private static FibonacciHeap<float, PathStep> toBeEvaluated;
        private static Dictionary<IGraphNode, HeapNode<float, PathStep>> nodeCatalogue;

        /// <summary>
        /// Finds all traversable nodes within cost of origin.
        /// </summary>
        /// <param name="target">The node to find nodes within range of</param>
        /// <param name="maximumCost">The maximum cost of traversing to any node in range</param>
        /// <param name="traverser">The ruleset for which nodes can be traversed and the cost of doing so</param>
        /// <returns> Steps that are traversable and within range of origin</returns>
        public static ICollection<PathStep> Area(IGraphNode origin, float maximumCost, ITraversable traverser = null)
        {
            List<PathStep> inRange = new List<PathStep>();

            // Enumerate over each step that is in range, add it to the collection
            foreach (PathStep step in Enumerate(origin, maximumCost, traverser))
                inRange.Add(step);

            return inRange;
        }

        /// <summary>
        /// Finds the cheapest, traversable path from origin to destination.
        /// </summary>
        /// <param name="origin">The beginning node of the path</param>
        /// <param name="destination">The end node of the path</param>
        /// <param name="maximumCost">The maximum cost allowed in order to find a path. -1 for no cost limit</param>
        /// <param name="traverser">The ruleset for which nodes can be traversed and the cost for doing so</param>
        /// <returns>The cheapest path from origin to destination. Returns null if no path exists OR no path exists within the 
        /// given cost constraint</returns>
        public static Path Between(IGraphNode origin, IGraphNode destination, float maximumCost = -1, ITraversable traverser = null)
        {
            return Between(origin,
                      s => s.Node == destination,       // Search until the returned step is the destination node
                      maximumCost,
                      traverser);
        }

        /// <summary>
        /// Finds the cheapest, traversable path from the origin to the first node that matches the given criteria.
        /// </summary>
        /// <param name="origin">The beginning node of the path</param>
        /// <param name="isTarget">Predicate that determines the desired node</param>
        /// <param name="maximumCost">The maximum cost allowed in order to find a path. -1 for no cost limit</param>
        /// <param name="traverser">The ruleset for which nodes can be traversed and the cost for doing so</param>
        /// <returns>The cheapest traversable path from origin to the first node that matches the given predicate. Returns null 
        /// if no path exists OR no path exists within the given cost constraint</returns>
        public static Path Between(IGraphNode origin, Func<PathStep, bool> isTarget, float maximumCost = -1, ITraversable traverser = null)
        {
            // Enumerate through nodes in cost order
            foreach (PathStep step in Enumerate(origin, maximumCost, traverser))
            {
                // If the step matches the condition, return a path from origin to this node
                if (isTarget(step))
                    return new Path(step);
            }

            // No traversable path
            return null;
        }

        /// <summary>
        /// Finds the cheapest, traversable path from origin to any of the given nodes.
        /// </summary>
        /// <param name="origin">The beginning node of the path</param>
        /// <param name="area">The nodes to find a path to</param>
        /// <param name="traverser">The ruleset for which nodes can be traversed and the cost for doing so</param>
        /// <returns>The cheapest, traversable path from origin to any of the given nodes. Returns null if no path 
        /// exists</returns>
        public static Path ToArea(IGraphNode origin, IEnumerable<IGraphNode> area, ITraversable traverser = null)
        {
            // Pathfind to cheapest of the nodes
            Path path = Between(origin,
                           s => area.Contains(s.Node),     // Search until the returned step is any of the nodes in the area
                           -1,                             // Cost doesn't matter
                           traverser);
            return path;
        }

        /// <summary>
        /// Checks if target is within range of origin
        /// </summary>
        /// <param name="origin">The node to check range from</param>
        /// <param name="target">The node to check for inclusion within the range</param>
        /// <param name="maximumCost">The size of the range to check, measured in the cost to traverse from the origin</param>
        /// <param name="traverser">The ruleset for which nodes can be traversed and the cost for doing so</param>
        /// <returns>Whether the target node is within range of the origin</returns>
        public static bool InRange(IGraphNode origin, IGraphNode target, float maximumCost, ITraversable traverser = null)
        {
            // Find path from origin to target
            Path path = Between(origin,
                           s => s.Node == target,        // Search until the returned step is the target node
                           maximumCost,
                           traverser);

            // Null = no path to target
            bool inRange = false;
            if (path != null)
                inRange = path.Cost <= maximumCost; // Is the path to target within cost?

            return inRange;
        }

        /// <summary>
        /// Iterates over affordable, traversable nodes in order of cost beginning at origin.
        /// </summary>
        /// <param name="origin">The node to begin iterating from</param>
        /// <param name="maximumCost">The maximum cost of nodes</param>
        /// <param name="traverser">The ruleset for which nodes can be traversed and the cost for doing so</param>
        public static IEnumerable<PathStep> Enumerate(IGraphNode origin, float maximumCost, ITraversable traverser = null)
        {
            evaluated = new HashSet<IGraphNode>();            // Nodes whose cost has been evaluated
            toBeEvaluated = new FibonacciHeap<float, PathStep>();    // Discovered nodes that have not yet been evaluated
            nodeCatalogue = new Dictionary<IGraphNode, HeapNode<float, PathStep>>();

            // Add origin to collection and iterate
            toBeEvaluated.Insert(0, new PathStep(origin, null, 0));
            while (toBeEvaluated.Count > 0)
            {
                // Remove current from unevaluated and add to evaluated
                PathStep current = toBeEvaluated.Pop();
                evaluated.Add(current.Node);

                // Don't bother evaluating nodes that are out of cost range
                if (maximumCost < 0 || current.CostTo <= maximumCost)
                {
                    yield return current;

                    // Checks each node adjacent to current, adds it to toBeEvaluated or updates it 
                    // if travelling through current is a better route
                    EvaluateAdjacent(current, traverser);
                }
            }
        }

        /// <summary>
        /// Partitions current's neighbours, adding them to the queue of nodes to be evaluated OR updating their
        /// cost and 'path to' information should they already exist in the queue.
        /// </summary>
        /// <param name="current">The node whose neighbous are to be checked</param>
        /// <param name="traverser">The ruleset for which nodes can be traversed and the cost for doing so</param>
        private static void EvaluateAdjacent(PathStep current, ITraversable traverser)
        {
            // Evaluate all adjacent nodes
            foreach (IGraphNode adjacent in current.Node.Nodes)
            {
                if (!evaluated.Contains(adjacent))        // Has adjacent already been evaluated?
                {
                    // Able to traverse to adjacent from current?
                    bool traversable = traverser != null ? traverser.IsTraversable(current.Node, adjacent) : true;
                    if (!traversable) continue;

                    // Cost to move from origin to adjacent with current route
                    float costIncrement = traverser != null ? traverser.Cost(current.Node, adjacent) : 1;
                    float costToAdjacent = current.CostTo + costIncrement;

                    InsertOrUpdate(current, adjacent, costToAdjacent);
                }
            }
        }

        /// <summary>
        /// Adds adjacent to the queue of nodes to be evaluated OR updates its
        /// cost and path to information should it already exist in the queue.
        /// </summary>
        private static void InsertOrUpdate(PathStep current, IGraphNode adjacent, float costToAdjacent)
        {
            // Is adjacent a newly discovered node...?
            if (!nodeCatalogue.ContainsKey(adjacent))
                InsertNode(adjacent, current, costToAdjacent);

            else
                UpdateQueue(adjacent, current, costToAdjacent);
        }

        private static void UpdateQueue(IGraphNode node, PathStep previous, float costTo)
        {
            HeapNode<float, PathStep> queueNode = nodeCatalogue[node];

            // Is the current path to this already discovered node a better path?
            if (costTo < queueNode.Value.CostTo)
            {
                // This path is best until now, record it.
                queueNode.Value.Previous = previous;
                queueNode.Value.CostTo = costTo;

                // Update order of queue to reflect updated cost
                toBeEvaluated.DecreasePriority(queueNode, costTo);
            }
        }

        /// <summary>
        /// Inserts the given step into the queue in order of cost
        /// </summary>
        private static void InsertNode(IGraphNode node, PathStep previous, float costTo)
        {
            PathStep step = new PathStep(node, previous, costTo);
            HeapNode<float, PathStep> queueNode = toBeEvaluated.Insert(costTo, step);
            nodeCatalogue.Add(node, queueNode);
        }
    }
}