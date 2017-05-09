# Pathfinding

Implementation of Dijkstra's shortest path algorithm with caller defined edge traversability and cost.

## Understanding Dijkstra's Algorithm

See [this](https://github.com/Clarksj4/RPGCampaign/wiki/Dijkstra's-Algorithm) page for information about this implementation of Dijkstra's shortest path Algorithm


## Finding a path

Using a graph data structure that has nodes which implement _IGraphNode_ is required. The following examples use a graph consisting of hexagonal tiles (hence the name: Hex).

    using Pathfinding;

    public class Hex : IGraphNode
    {
        public IEnumerable<IGraphNode> Nodes { get { /* Return connected nodes */ } }

        ...
    }

Finding a path happens by calling one of the static _Pathfind_ class methods. The simplest use: finding a path between two nodes in the same graph. There is no maximum cost which the path must comply to, each edge has a cost of 1, and every edge in the graph is traversable.

    using Pathfinding;

    public void PathfindExample()
    {
        Path path = Pathfind.Between(hex1, hex2);

        // Check there is a path between the nodes
        if (path != null)
        {
            // ... Do something with the path ...
            // ... Draw the path, walk the path etc ...
        } 
    }

### Cost and Traversability

Implementing a ruleset for the cost and traversability of nodes allows for further functionality.

    public class ExampleTraverser : ITraversable
    {
        ...

        public bool IsTraversable(IGraphNode begin, IGraphNode next)
        {
            // Cast to type of node used in graph implementation (Hex is an example)
            Hex beginHex = (Hex)begin;
            Hex nextHex = (Hex)next;
        
            // ... Check properties of hex ...
            // ... Is next occupied? Is there a wall between hexes? etc ...

            // ... Return true if traversable ...
            return true;
        }

        public float Cost(IGraphNode begin, IGraphNode next)
        {
            // Cast to type of node used in graph implementation (Hex is an example)
            Hex beginHex = (Hex)begin;
            Hex nextHex = (Hex)next;

            // ... Check properties of hex...
            // ... Is there a change in elevation? Is next difficult terrain? etc ...

            // ... Return cost ...
            return 2;
        }

        ...
    }

An overload of the _Pathfind.Between()_ method accepts the new traverser object as well as a maximum cost to search within. Using -1 for the maximum cost means the method will find a path, if one exists, no matter the cost. Alternately, the search for a path can be cut short if it extends outside of a given cost (greater than -1). Should this occur the method will return null.

    ExampleTraverser exampleTraverser;
    float maximumCost;

    public void PathfindExample()
    {
        // Use a traverser to avoid obstacles, and manipulate which edges are optimal
        Path path = Pathfind.Between(hex1, hex2, maximumCost, exampleTraverser);
        
        // No path between nodes within cost
        if (path == null)
        {
            // ... Do something ...
            // ... Find a path to a different node, increase maximum cost, etc ...
        } 

        // There is a path within cost
        else
        {
            // ... Do something with the path ...
            // ... Draw the path, walk the path etc ...
        }
    }

### Areas

It is possible to get all nodes within _cost_ of a node. One use of this functionality is to find a path to any node in an area.

    public void GetAreaExample()
    {
        // Get all traversable nodes within 14 cost of hex1
        ICollection<PathStep> area = Pathfind.Area(hex1, maximumCost, exampleTraverser1);

        // ... Do something with the area ...
        // ... Check if it contains a given node, etc ...
        if (area.Contains(hex2))
        {
            // ... Do something else ...
        }

        // Alternately, you can get a path to the quickest to reach node in the area
        Path path = Pathfind.ToArea(hex3, area, exampleTraverser2);

        // Check if there is a traversable path
        if (path != null)
        ...
    }

### Range check

When only concerned if a node is within a _cost_ of another, it is more efficient to call the _Pathfind.InRange()_ method, as opposed to getting an entire area.

    public void InRangeExample()
    {
        // Is hex2 within 14 cost of hex1 and are the edges between it traversable? 
        bool inRange = Pathfind.InRange(hex1, hex2, maximumCost, exampleTraverser);
  
        if (inRange)
        {
            // ... Do something ...
            // ... Shoot, run away, etc
        }
    }

### Custom target node

A condition can be defined instead of a target node. In this instance, the returned path is the cheapest path to the first node that matches the given condition.

    public void CustomNodeConditionExample()
    {
        // Find path to first hex where SomeCondition() returns true, regardless of cost
        Path path = Pathfind.Between(hex1, 
                                     s => (Hex(s.Node)).SomeCondition(), 
                                     -1, 
                                     exampleTraverser);  

        if (path != null)
        {
            // ... Do something with the path ...
            // ... Draw the path, walk the path etc ...
        }
    }

### Enumerate

To act upon each node as it is iterated by the pathfinding algorithm, the _Enumerate_ method can be used.

    public void EnumerateExample()
    {
        // Iterate over each node within the given cost of the give node
        IEnumerable<PathStep> steps = Pathfind.Enumerate(hex1, 
                                                         maximumCost, 
                                                         exampleTraverser)

        foreach (PathStep step in steps)
        {
            // Cast to type of node used in graph implementation (Hex is an example)
            (Hex) hex = (Hex)step.Node;

            // ... Do something ...
            // ... Change colour of node, change elevation, etc ...
            hex.DoSomething();
        }
    }
