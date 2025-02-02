using Pathfinding;

public static class ABPathExt
{

    public static int MoveLength( this ABPath path )
    {
        return path.path.Count - 1;
    }

}