using Pathfinding;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardTraversalProvider : ITraversalProvider
{
    public Actor Traverser = null;

    public Dictionary<GraphNode, Actor> Occupied = new Dictionary<GraphNode, Actor>();

    public BoardTraversalProvider()
    {

    }

    public bool IsBlocked( Vector2Int position )
    {
        var node = AstarPath.active.GetNearest( position.GetWorldPosition() );
        return Occupied.ContainsKey( node.node );
    }

    public bool IsBlocked( Vector3 position )
    {
        var node = AstarPath.active.GetNearest( position );
        return Occupied.ContainsKey( node.node );
    }

    public BoardTraversalProvider( BoardTraversalProvider other )
    {
        this.Traverser = other.Traverser;
        this.Occupied = other.Occupied.ToDictionary( x => x.Key, x => x.Value );
    }

    public bool Block( Actor actor, Vector3 position )
    {
        var node = AstarPath.active.GetNearest( position );

        if( Occupied.ContainsKey( node.node ) )
            return false;
        Occupied.Add( node.node, actor );
        return true;
    }

    public void Unblock( Vector3 position )
    {
        var node = AstarPath.active.GetNearest( position );
        if( !Occupied.ContainsKey( node.node ) )
            return;
        Occupied.Remove( node.node );
    }

    public bool CanTraverse( Path path, GraphNode node )
    {
        bool actorBlocks = false;
        Actor actorOn = null;
        if( Occupied.TryGetValue( node, out actorOn ) )
        {
            actorBlocks = actorOn != Traverser;
        }

        // Make sure that the node is walkable and that the 'enabledTags' bitmask
        // includes the node's tag.
        return node.Walkable && ( path.enabledTags >> (int) node.Tag & 0x1 ) != 0 && !actorBlocks;
        // alternatively:
        // return DefaultITraversalProvider.CanTraverse(path, node);
    }

    public bool CanTraverse( Path path, GraphNode from, GraphNode to )
    {
        return CanTraverse( path, to );
    }

    public uint GetTraversalCost( Path path, GraphNode node )
    {
        // The traversal cost is the sum of the penalty of the node's tag and the node's penalty
        return path.GetTagPenalty( (int) node.Tag ) + node.Penalty;
        // alternatively:
        // return DefaultITraversalProvider.GetTraversalCost(path, node);
    }

    // This can be omitted in Unity 2021.3 and newer because a default implementation (returning true) can be used there.
    public bool filterDiagonalGridConnections
    {
        get
        {
            return true;
        }
    }
}