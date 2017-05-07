using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding
{
    /// <summary>
    /// A path containing the order of steps to be traversed, and the cost of traversing them
    /// </summary>
    public class Path : IEnumerable<PathStep>
    {
        /// <summary>
        /// The first step in this path
        /// </summary>
        public IGraphNode Origin { get { return steps.First().Node; } }

        /// <summary>
        /// The last step in this path
        /// </summary>
        public IGraphNode Destination { get { return steps.Last().Node; } }

        /// <summary>
        /// The total cost of moving along this path
        /// </summary>
        public float Cost { get { return steps.Last().CostTo; } }

        /// <summary>
        /// The number of steps in this path
        /// </summary>
        public int Count { get { return steps.Count; } }

        private LinkedList<PathStep> steps;

        /// <summary>
        /// An empty path
        /// </summary>
        public Path()
        {
            steps = new LinkedList<PathStep>();
        }

        /// <summary>
        /// Creates a path by walking backwards from the given step
        /// </summary>
        /// <param name="final">The final step in the path</param>
        public Path(PathStep final)
            : this()
        {
            // Iterate through given steps's chain of steps
            PathStep walker = final;
            while (walker != null)
            {
                // Add as first step in path
                steps.AddFirst(walker);
                walker = walker.Previous;
            }
        }

        /// <summary>
        /// Adds a step to the path
        /// </summary>
        public void AddStep(PathStep step)
        {
            steps.AddLast(step);
        }

        /// <summary>
        /// Returns a new path containing all steps in this path less than the given time units
        /// </summary>
        /// <param name="timeUnits">The maximum cost of the steps in the sub path</param>
        public Path Truncate(float timeUnits)
        {
            // Iterate through steps adding those that are less that given time units
            Path subPath = new Path();
            foreach (PathStep step in this)
            {
                if (step.CostTo <= timeUnits)
                    subPath.AddStep(step);

                // Stop once steps are out of time units range
                else
                    break;
            }

            // All steps that are within cost
            return subPath;
        }

        public IEnumerator<PathStep> GetEnumerator()
        {
            return steps.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return steps.GetEnumerator();
        }
    }
}