using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameModel
{
    public List<GridCell> grid = new List<GridCell>();
    private List<Tile> tiles = new List<Tile>();

    private int scorePoints;
    private int scoreMultiplier;
    private int currentLevel;

    public int[] gpSeries;
    private HashSet<Tile> neighbors = new HashSet<Tile>();
    private List<Tile> connectedTiles = new List<Tile>();
    private int mergeSumResult;
    private int currentMergeSum;

    private TilesTheme currentTheme;

    private Tile startTile;
    private bool canClick = true;

    #region CONSTRUCTORS
    public GameModel() 
    {
        
    }
    #endregion

    public void TouchStarted(Tile tile)
    {
        connectedTiles.Clear();
        neighbors.Clear();
        mergeSumResult = 0;

        startTile = tile;
        connectedTiles.Add(tile);
        currentMergeSum = tile.Value;
        mergeSumResult = tile.ValueId;
        CheckForNeighbors(tile);
    }
    public void TouchIsHeld()
    {
        
    }
    public void TouchEnded()
    {
        startTile = null;
        if (connectedTiles.Count > 1)
        {
            IncreaseMatchScore(currentMergeSum);
        }
        
    }

    public void OnMerge()
    {
        IncreaseMatchScore(currentMergeSum);
        
    }


    public List<GridCell> GetListOfFreeGridCells()
    {
        return grid.Where(n => n.OccupiedTile == null).ToList();
    }

    public void AddTile(Tile _tile, GridCell _gridCell, int _id)
    {
        _tile.SetTile(_gridCell);
        _tile.Init(currentTheme.tilesTheme[_id], currentTheme.showText, GetValueFromGeometricSeries(_id), _id);
        tiles.Add(_tile);
        _gridCell.OccupiedTile = _tile;
    }

    public void SetUpValueNumbers(bool _useGS,int _count)
    {
        if(_useGS)
        {
            gpSeries = GeometricSeries(_count);
        }
        else
        {
            gpSeries = NormalNumbers(_count);
        }
    }

    public int GetValueFromGeometricSeries(int _id)
    {
        return gpSeries[_id];
    }

    public void SetCurrentTheme(TilesTheme theme)
    {
        currentTheme = theme;
    }

    public void GenerateGrid(int _width, int _height)
    {
        for (int y = 0; y < _width; y++)
        {
            for (int x = 0; x < _height; x++)
            {
                var gridcell = new GridCell(x, y);
                grid.Add(gridcell);
            }
        }
    }

    public IEnumerator MergeTiles()
    {
        foreach (var tile in connectedTiles)
        {
            tiles.Remove(tile);
            tile.DeactivateLineLink();
            tile.MoveTo(connectedTiles.Last().Pos, 0.39f);

            tile.Obliterate(0.45f);
        }
        yield return null;
    }

    public IEnumerator MakeTilesFall()
    {
        connectedTiles.Clear();
        foreach (var gridCell in grid)
        {
            if (gridCell.OccupiedTile == null)
            {
                var gridsAbove0 = grid.Where(x => x.Pos.x == gridCell.Pos.x && x.Pos.y >= gridCell.Pos.y && x.OccupiedTile != null).ToList();

                if (gridsAbove0.Count != 0)
                {
                    gridsAbove0.First().OccupiedTile.MoveTo(gridCell.Pos, 0.4f);
                    gridCell.OccupiedTile = gridsAbove0.First().OccupiedTile;
                    gridCell.OccupiedTile.SetTile(gridCell);
                    //gridCell.OccupiedTile.GridCell = gridCell;
                    gridsAbove0.First().OccupiedTile = null;
                }
            }
            yield return null;
        }
    }

    private void IncreaseMatchScore(int addedScore)
    {
        var result = addedScore * scoreMultiplier;
        scorePoints += (int)result;
        
        // check level logic
    }

    public int GetMergeValue()
    {
        //CONVERT FROM ID BEFORE RETURNING
        return mergeSumResult;
    }
    public List<Tile> GetConnectedTiles()
    {
        return connectedTiles;
    }
    public Tile GetStartTile()
    {
        return startTile;
    }
    public bool CheckForConnectedTiles()
    {
        return connectedTiles.Count > 1;
    }

    private int[] GeometricSeries(int _count)
    {
        int[] gpSeries = new int[_count];
        int value = 2;
        double gpn;

        // Loop to generate the geometric progression (G.P.) series
        for (int i = 1; i < _count; i++)
        {
            gpn = Math.Pow(value, i);  // Calculate each term of the series

            gpSeries[i - 1] = ((int)gpn);  // Display the terms
        }
        return gpSeries;
    }
    private int[] NormalNumbers(int _count)
    {
        int[] gpSeries = new int[_count];

        for (int i = 1; i <= _count; i++)
        {
            gpSeries[i] = i;
        }
        return gpSeries;
    }

    public void CheckForPossibilities(Tile _currentTile)
    {
        if (connectedTiles.Last() != _currentTile && neighbors.Contains(_currentTile))
        {
            if (connectedTiles.Last().Value != _currentTile.Value || startTile == _currentTile && connectedTiles.Count > 2)
            {
                return;
            }

            // Check if it returns a Node
            if (connectedTiles.Count > 1)
            {
                if (connectedTiles[connectedTiles.Count - 2] == _currentTile || startTile == _currentTile)
                {
                    connectedTiles.Remove(connectedTiles.Last());
                    CheckForNeighbors(_currentTile);
                    currentMergeSum -= _currentTile.Value;
                    if (gpSeries.Contains(currentMergeSum) && currentMergeSum != 2)
                    {
                        mergeSumResult--;
                    }
                    _currentTile.DeactivateLineLink();

                    return;
                }
            }

            // Check Pos of current tile to give the angle of the line of the previous tile // NEEDS REWORK
            if (!connectedTiles.Contains(_currentTile))
            {
                SetAngleToLine(_currentTile, connectedTiles.Last());

                connectedTiles.Add(_currentTile);
                neighbors.Clear();
                CheckForNeighbors(_currentTile);
                currentMergeSum += _currentTile.Value;
                // MergeNumber
                if (gpSeries.Contains(currentMergeSum) && currentMergeSum != 2)
                {
                    mergeSumResult++;
                }
            }
        }
        return;
    }

    private void CheckForNeighbors(Tile currentTile)
    {
        foreach (var gridCell in grid)
        {
            //NEED PERFORMANCE CHECK
            //left
            if (gridCell.Pos.x == currentTile.Pos.x - 1 && gridCell.Pos.y == currentTile.Pos.y)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //left-up
            else if (gridCell.Pos.x == currentTile.Pos.x - 1 && gridCell.Pos.y == currentTile.Pos.y + 1)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //up
            else if (gridCell.Pos.x == currentTile.Pos.x && gridCell.Pos.y == currentTile.Pos.y + 1)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //up-right
            else if (gridCell.Pos.x == currentTile.Pos.x + 1 && gridCell.Pos.y == currentTile.Pos.y + 1)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //right
            else if (gridCell.Pos.x == currentTile.Pos.x + 1 && gridCell.Pos.y == currentTile.Pos.y)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //right-down
            else if (gridCell.Pos.x == currentTile.Pos.x + 1 && gridCell.Pos.y == currentTile.Pos.y - 1)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //down
            else if (gridCell.Pos.x == currentTile.Pos.x && gridCell.Pos.y == currentTile.Pos.y - 1)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //down-left
            else if (gridCell.Pos.x == currentTile.Pos.x - 1 && gridCell.Pos.y == currentTile.Pos.y - 1)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
        }
    }

    private void SetAngleToLine(Tile currentTile, Tile previousTile)
    {
        //left
        if (currentTile.Pos.x < previousTile.Pos.x && currentTile.Pos.y == previousTile.Pos.y)
        {
            previousTile.ActivateLineLink(90);
        }
        //left-up
        if (currentTile.Pos.x < previousTile.Pos.x && currentTile.Pos.y > previousTile.Pos.y)
        {
            previousTile.ActivateLineLink(45);
        }
        //up
        if (currentTile.Pos.x == previousTile.Pos.x && currentTile.Pos.y > previousTile.Pos.y)
        {
            previousTile.ActivateLineLink(0);
        }
        //up-right
        if (currentTile.Pos.x > previousTile.Pos.x && currentTile.Pos.y > previousTile.Pos.y)
        {
            previousTile.ActivateLineLink(-45);
        }
        //right
        if (currentTile.Pos.x > previousTile.Pos.x && currentTile.Pos.y == previousTile.Pos.y)
        {
            previousTile.ActivateLineLink(-90);
        }
        //right-down
        if (currentTile.Pos.x > previousTile.Pos.x && currentTile.Pos.y < previousTile.Pos.y)
        {
            previousTile.ActivateLineLink(-135);
        }
        //down
        if (currentTile.Pos.x == previousTile.Pos.x && currentTile.Pos.y < previousTile.Pos.y)
        {
            previousTile.ActivateLineLink(-180);
        }
        //down-left
        if (currentTile.Pos.x < previousTile.Pos.x && currentTile.Pos.y < previousTile.Pos.y)
        {
            previousTile.ActivateLineLink(135);
        }
    }
}
