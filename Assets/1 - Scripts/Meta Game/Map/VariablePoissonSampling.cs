using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class VariablePoissonSampling
{
    [System.Serializable]
    public struct Cell
    {
        public int PointIndex;
        public List<int> InfluenceList;

        public void Init()
        {
            InfluenceList = new List<int>();
        }

        public void AddActive(int index)
        {
            InfluenceList.Add(index);
        }
    }

    [System.Serializable]
    public struct Point
    {
        public int CellX, CellY;
        public Vector2 Position;
        public Vector2 NormalizedPosition;
        public float Radius;

        public Vector3 Position3
        {
            get
            {
                return new Vector3(Position.x, 0, Position.y);
            }
        }
    }

    public float MinRadius = .5f, MaxRadius = 2;
    public float FieldWidth = 10, FieldHeight = 10;
    public int SpawnSamples = 10;
    [HideInInspector]
    public List<Point> Points = new List<Point>();

    private int _gridWidth, _gridHeight;
    private Cell[,] _minRadiiActiveList;
    private float _cellSize;
    private System.Random _random;

    private bool _usedSeed = false;
    private int _seed;
    private Random.State _originalSeed;

    public void SetupSeed(int seed)
    {
        _usedSeed = true;
        _seed = seed;
    }

    public void ClearSeed()
    {
        _usedSeed = false;
    }

    public Point NewPoint(Vector2 position, float radius )
    {
        int cellX = Mathf.RoundToInt(position.x / _cellSize);
        int cellY = Mathf.RoundToInt(position.y / _cellSize);
        return new Point() { CellX = cellX, CellY = cellY, Position = position, NormalizedPosition = new Vector2(position.x/FieldWidth, position.y / FieldHeight), Radius = radius };
    }

    bool IsCellInGrid(int x, int y)
    {
        return x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight;
    }

    public void CalculatePoints()
    {
        if( _usedSeed )
        {
            _originalSeed = Random.state;
            Random.InitState( _seed );
        }

        Points.Clear();
        
        float sqrt2 = Mathf.Sqrt(2f);
        float sqrt2Recip = 1f / sqrt2;

        void PushInfluence(Point point)
        {
            Points.Add(point);
            int pointIndex = Points.Count - 1;

            _minRadiiActiveList[point.CellX, point.CellY].PointIndex = pointIndex;

            int gridRadius = Mathf.CeilToInt(point.Radius / _cellSize) + 1;
            for( int x = point.CellX - gridRadius; x <= point.CellX + gridRadius; x++)
            {
                for (int y = point.CellY - gridRadius; y <= point.CellY + gridRadius; y++)
                {
                    if (!IsCellInGrid(x, y))
                        continue;
                    _minRadiiActiveList[x, y].InfluenceList.Add(pointIndex);
                }
            }
        }

        _cellSize = MinRadius / sqrt2;
        _gridWidth = Mathf.CeilToInt(FieldWidth / _cellSize);
        _gridHeight = Mathf.CeilToInt(FieldHeight / _cellSize);
        _minRadiiActiveList = new Cell[_gridWidth, _gridHeight];
        for( int x = 0; x < _gridWidth; ++x)
        {
            for (int y = 0; y < _gridHeight; ++y)
            {
                var cell = new Cell();
                cell.Init();
                _minRadiiActiveList[x, y] = cell;
            }
        }

        List<Point> spawnList = new List<Point>();

        var firstPoint = NewPoint(new Vector2(FieldWidth / 2, FieldHeight / 2), Random.Range(MinRadius, MaxRadius));
        PushInfluence(firstPoint);
        spawnList.Add(firstPoint);

        //Insert first item into spawnList
        while (spawnList.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnList.Count);
            Point spawnCentre = spawnList[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < SpawnSamples; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                float pointRadius = Random.Range(MinRadius, MaxRadius);
                float pointOffset = Random.Range(spawnCentre.Radius, spawnCentre.Radius * 2);
                Vector2 candidate = spawnCentre.Position + dir * pointOffset;
                if (IsValid(candidate, FieldWidth, FieldHeight, pointRadius))
                {
                    var newPt = NewPoint(new Vector2(candidate.x, candidate.y), pointRadius);
                    PushInfluence(newPt);
                    spawnList.Add(newPt);
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                spawnList.RemoveAt(spawnIndex);
            }
        }

        if( _usedSeed )
        {
            Random.state = _originalSeed;
        }
    }

    public List<Point> GetPointCopy(Rect bounds)
    {
        float widthRecip = 1f / FieldWidth;
        float heightRecip = 1f / FieldHeight;
        List<Point> pointsCopy = new List<Point>(Points);
        for( int i = 0; i < pointsCopy.Count; ++i)
        {
            Point point = pointsCopy[i];
            point.Position = bounds.min + point.Position;
            pointsCopy[i] = point;
        }
        return pointsCopy;
    }

    bool IsValid(Vector2 candidate, float sampleRegionX, float sampleRegionY, float radius)
    {
        int cellX = Mathf.RoundToInt(candidate.x / _cellSize);
        int cellY = Mathf.RoundToInt(candidate.y / _cellSize);

        if (candidate.x < 0f || candidate.y < 0f || candidate.x > FieldWidth || candidate.y > FieldHeight)
            return false;
        if (cellX >= 0 && cellX < _gridWidth && cellY >= 0 && cellY < _gridHeight)
        {
            int gridRadius = Mathf.CeilToInt(radius / _cellSize) + 1;
            for (int x = cellX - gridRadius; x <= cellX + gridRadius; x++)
            {
                for (int y = cellY - gridRadius; y <= cellY + gridRadius; y++)
                {
                    if (!IsCellInGrid(x, y))
                        continue;

                    var cell = _minRadiiActiveList[x, y];
                    var influenceList = cell.InfluenceList;
                    for (int i = 0; i < influenceList.Count; ++i)
                    {
                        int pointIndex = influenceList[i];
                        var influencePoint = Points[pointIndex];
                        float sqrDst = (candidate - influencePoint.Position).sqrMagnitude;
                        if (sqrDst < radius * radius || sqrDst < influencePoint.Radius * influencePoint.Radius)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

}
