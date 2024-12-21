using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GridStarNode
{
    // Change this depending on what the desired size is for each element in the grid
    public static int NODE_SIZE = 32;
    public int XPos, YPos;
    public GridStarNode Parent;
    public float DistanceToTarget;
    public float Cost;
    public float Weight;
    public bool Walkable;
    public bool Occupied;

    public GridStarNode( GridStarNode other )
    {
        this.XPos = other.XPos;
        this.YPos = other.YPos;
        this.Weight = other.Weight;
        this.Walkable = other.Walkable;
        this.Occupied = other.Occupied;
    }

    public float F
    {
        get
        {
            if (DistanceToTarget != -1 && Cost != -1)
                return DistanceToTarget + Cost;
            else
                return -1;
        }
    }

    public GridStarNode( int x, int y, bool walkable, float weight = 1)
    {
        XPos = x;
        YPos = y;
        DistanceToTarget = -1;
        Cost = 1;
        Weight = weight;
        Walkable = walkable;
        Occupied = false;
    }

    public bool Equals( GridStarNode other )
    {
        return this.XPos == other.XPos && this.YPos == other.YPos;
    }

    public Vector3 WorldPosition()
    {
        return new Vector3( XPos + .5f, 0f, YPos + .5f );
    }
}

[System.Serializable]
public class GridStar
{
    public GridStarNode[,] Grid = null;
    public int Width, Height;

    public GridStar( GridStar other )
    {
        this.Width = other.Width;
        this.Height = other.Height;
        this.Grid = new GridStarNode[Width, Height];
        for( int x = 0; x < Width; x++ )
        {
            for( int y = 0; y < Height; y++ )
            {
                this.Grid[x, y] = new GridStarNode( other.Grid[x,y] );
            }
        }
    }

    public GridStar( int width, int height )
    {
        this.Width = width;
        this.Height = height;
        Grid = new GridStarNode[width, height];
        for( int x = 0; x < width; x++ )
        {
            for( int y = 0; y < height; y++ )
            {
                Grid[x, y] = new GridStarNode( x, y, true, 1f );
            }
        }
    }

    public Stack<GridStarNode> GetPath( Vector2Int start, Vector2Int end, bool allowOccupied = false )
    {
        //Calculate path
        GridStarNode startNode = Grid[start.x, start.y];
        GridStarNode endNode = Grid[end.x, end.y];
        return FindPath( startNode, endNode, allowOccupied );
    }

    public Stack<GridStarNode> FindPath( GridStarNode start, GridStarNode end, bool allowOccupied )
    {
        //Return empty path if start same as end.
        if( start == end )
        {
            return new Stack<GridStarNode>();
        }

        Stack<GridStarNode> Path = new Stack<GridStarNode>();
        List<GridStarNode> OpenList = new List<GridStarNode>();
        List<GridStarNode> ClosedList = new List<GridStarNode>();
        GridStarNode[,] adjacencies = new GridStarNode[3, 3];
        GridStarNode current = start;

        // add start node to Open List
        OpenList.Add(start);

        while (OpenList.Count != 0 && !ClosedList.Exists(x => x.Equals( end ) ) )
        {
            current = OpenList[0];
            OpenList.Remove(current);
            ClosedList.Add(current);
            GetAdjacentNodes(current, adjacencies);

            foreach (GridStarNode n in adjacencies)
            {
                if( n == null )
                    continue;
                if (!ClosedList.Contains(n) && n.Walkable && (!n.Occupied || allowOccupied) )
                {
                    if (!OpenList.Contains(n))
                    {
                        n.Parent = current;
                        n.DistanceToTarget = (n.WorldPosition() - end.WorldPosition()).magnitude;
                        n.Cost = n.Weight + n.Parent.Cost;
                        OpenList.Add(n);
                        OpenList = OpenList.OrderBy(node => node.F).ToList<GridStarNode>();
                    }
                }
            }
        }

        // construct path, if end was not closed return null
        if (!ClosedList.Exists(x => x == end ) )
            return null;

        // if all good, return path
        GridStarNode temp = ClosedList[ClosedList.IndexOf(current)];
        if (temp == null) return null;
        do
        {
            Path.Push(temp);
            temp = temp.Parent;
        } while (temp != start && temp != null);
        return Path;
    }

    private void GetAdjacentNodes( GridStarNode node, GridStarNode[,] adjacencies )
    {
        adjacencies[0, 0] = null;
        MakeAdjacent( node, 1, 0, adjacencies );
        MakeAdjacent( node, -1, 0, adjacencies );
        MakeAdjacent( node, 0, 1, adjacencies );
        MakeAdjacent( node, 0, -1, adjacencies );
        /*
        for( int x = -1; x <= 1; x++ )
        {
            for( int y = -1; y <= 1; y++ )
            {
                int dX = node.XPos + x;
                int dY = node.YPos + y;

                if( dX < 0 && dY < 0 )
                    continue;
                if( dX >= Width && dY >= Height )
                    continue;

                if( !IsValidCoord( dX, dY ) )
                    adjacencies[x+1, y+1] = null;
                else
                    adjacencies[x+1, y+1] = Grid[dX, dY];
            }
        }
        */
    }

    private void MakeAdjacent( GridStarNode node, int dx, int dy , GridStarNode[,] adjacencies )
    {
        int dX = node.XPos + dx;
        int dY = node.YPos + dy;

        if( dX < 0 && dY < 0 )
            return;
        if( dX >= Width && dY >= Height )
            return;

        if( !IsValidCoord( dX, dY ) )
            adjacencies[dx + 1, dy + 1] = null;
        else
            adjacencies[dx + 1, dy + 1] = Grid[dX, dY];
    }

    public bool IsValidCoord( int x, int y )
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public bool IsValidCoord( Vector2Int pos )
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < Width && pos.y < Height;
    }

    public void SetCellOccupied(Vector2Int cell, bool occupied)
    {
        GridStarNode gridCell = Grid[cell.x, cell.y];
        gridCell.Occupied = occupied;
    }

    public bool CellWalkable( Vector2Int cell )
    {
        GridStarNode gridCell = Grid[cell.x, cell.y];
        return gridCell.Walkable && !gridCell.Occupied;
    }
}
