using System.Collections.Generic;

namespace Pathfinding
{
    /// <summary>
    /// Interface for pathfinding node
    /// </summary>
    public interface IGraphNode
    {
        /// <summary>
        /// Nodes accessible from this one
        /// </summary>
        IEnumerable<IGraphNode> Nodes { get; }
    }
}
