using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameModel
{
    #region VARIABLES
    public List<GridCell> grid = new List<GridCell>();
    private List<Tile> tiles = new List<Tile>();

    private int scorePoints;
    private int scoreMultiplier = 1;
    private int currentLevel = 1;
    private int progress;

    public int[] gpSeries;
    private HashSet<Tile> neighbors = new HashSet<Tile>();
    private List<Tile> connectedTiles = new List<Tile>();

    private TilesTheme currentTheme;
    private Tile startTile;

    private bool merging = false;
    private bool canTouch = true;

    private GridCell mergedGridCell;
    private int mergeSumResult;
    #endregion VARIABLES

    #region PROPERTIES
    public GridCell MergedGridCell { get => mergedGridCell; }
    public int MergeSumResult { get => mergeSumResult; }
    public bool Merging { get => merging; }
    public TilesTheme CurrentTheme { get => currentTheme; }
    public int ScorePoints { get => scorePoints;}
    public int ScoreMultiplier { get => scoreMultiplier;}
    public int CurrentLevel { get => currentLevel;}
    public int Progress { get => progress;}
    #endregion PROPERTIES

    public void TouchStarted(Tile _tile)
    {
        // Set Start Tile -> Start Tile Anim -> Check For Neighbors
        if (!canTouch) return; 
        connectedTiles.Clear();
        neighbors.Clear();
        mergeSumResult = 0;

        startTile = _tile;
        _tile.Expand();
        connectedTiles.Add(_tile);
        mergeSumResult = _tile.ValueId;
        CheckForNeighbors(_tile);
        canTouch = false;

        AudioManager.Instance.ConnectSound(1f);
    }

    public void TouchIsHeld(Tile _tile)
    {
        if(canTouch) return;
        // Check for possible connections
        CheckForConnections(_tile);
    }

    public void TouchEnded()
    {
        if(canTouch) return ;
        // Check if there are Connections to Merge.
        if (connectedTiles.Count > 1)
        {
            IncreaseMatchScore();
            foreach (Tile tile in connectedTiles)
            {
                if(tile != null)
                {
                    tile.Contract();
                }
            }
            merging = true;
        }
        
        else if(startTile != null)
        {
            startTile.Contract();
            canTouch = true;
            merging = false;
        }
        
        startTile = null;
    }

    public void AddTile(Tile _tile, GridCell _gridCell, int _id)
    {
        _tile.SetTile(_gridCell);
        _tile.Init(CurrentTheme.tilesTheme[_id], CurrentTheme.showText, GetValueFromGeometricSeries(_id), _id);
        tiles.Add(_tile);
        _gridCell.OccupiedTile = _tile;
    }

    public void SetUpValueNumbers(bool _useGS, int _count)
    {
        if (_useGS)
        {
            gpSeries = GeometricSeries(_count);
        }
        else
        {
            gpSeries = NormalNumbers(_count);
        }
    }
    
    public void SetCurrentTheme(TilesTheme _theme)
    {
        currentTheme = _theme;
        foreach (var tile in tiles)
        {
            tile.ChangeTheme(_theme);
        }
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
            if (tile == null) break;
            tiles.Remove(tile);
            tile.DeactivateLineLink();
            tile.MoveTo(connectedTiles.Last().Pos, 0.22f);
            tile.Obliterate();
        }
        mergedGridCell = connectedTiles.Last().GridCell;
        yield return null;
    }

    public IEnumerator MakeTilesFall(float _speed)
    {
        foreach (var gridCell in grid)
        {
            if (gridCell.OccupiedTile == null)
            {
                var gridsAbove0 = grid.Where(x => x.Pos.x == gridCell.Pos.x && x.Pos.y >= gridCell.Pos.y && x.OccupiedTile != null).ToList();

                if (gridsAbove0.Count != 0)
                {
                    gridsAbove0.First().OccupiedTile.MoveTo(gridCell.Pos, _speed);
                    gridCell.OccupiedTile = gridsAbove0.First().OccupiedTile;
                    gridCell.OccupiedTile.SetTile(gridCell);
                    gridsAbove0.First().OccupiedTile = null;
                }
            }
            yield return null;
        }
        canTouch = true;
    }

    public void FinishedMerging()
    {
        connectedTiles.Clear();
        merging = false;
    }

    public void IncreaseMatchScore()
    {
        var value = GetValueFromGeometricSeries(GetMergeValue());
        var result = value * ScoreMultiplier;
        scorePoints += (int)result;
        progress += (int)result;
        if(progress >= 100)
        {
            currentLevel++;
            progress = 0;
            if (currentLevel == 4)
            {
                scoreMultiplier++;
            }
            else if (currentLevel == 8)
            {
                scoreMultiplier++;
            }
        }
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

    #region PERFORMANCE KILLERS :(
    private void CheckForConnections(Tile _currentTile)
    {
        // Check if it's going back one tile
        if (connectedTiles.Last() != _currentTile && neighbors.Contains(_currentTile))
        {
            if (connectedTiles.Last().Value != _currentTile.Value || startTile == _currentTile && connectedTiles.Count > 2)
            {
                return;
            }

            if (connectedTiles.Count > 1)
            {
                if (connectedTiles[connectedTiles.Count - 2] == _currentTile || startTile == _currentTile)
                {
                    AudioManager.Instance.ConnectSound(0.8f);

                    connectedTiles.Last().Contract();
                    connectedTiles.Remove(connectedTiles.Last());
                    CheckForNeighbors(_currentTile);
                    _currentTile.DeactivateLineLink();

                    return;
                }
            }

            // Connects, sets the angle of the previous tile
            if (!connectedTiles.Contains(_currentTile))
            {
                SetAngleToLine(_currentTile, connectedTiles.Last());

                var x = (connectedTiles.Count - 1) * 0.2f;
                AudioManager.Instance.ConnectSound(1f + x);

                connectedTiles.Add(_currentTile);
                _currentTile.Expand();
                neighbors.Clear();
                CheckForNeighbors(_currentTile);
            }
        }
        return;
    }
    private void CheckForNeighbors(Tile _currentTile)
    {
        neighbors.Clear();
        foreach (var gridCell in grid)
        {
            //NEEDS PERFORMANCE CHECK
            //left
            if (gridCell.Pos.x == _currentTile.Pos.x - 1 && gridCell.Pos.y == _currentTile.Pos.y)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //left-up
            else if (gridCell.Pos.x == _currentTile.Pos.x - 1 && gridCell.Pos.y == _currentTile.Pos.y + 1)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //up
            else if (gridCell.Pos.x == _currentTile.Pos.x && gridCell.Pos.y == _currentTile.Pos.y + 1)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //up-right
            else if (gridCell.Pos.x == _currentTile.Pos.x + 1 && gridCell.Pos.y == _currentTile.Pos.y + 1)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //right
            else if (gridCell.Pos.x == _currentTile.Pos.x + 1 && gridCell.Pos.y == _currentTile.Pos.y)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //right-down
            else if (gridCell.Pos.x == _currentTile.Pos.x + 1 && gridCell.Pos.y == _currentTile.Pos.y - 1)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //down
            else if (gridCell.Pos.x == _currentTile.Pos.x && gridCell.Pos.y == _currentTile.Pos.y - 1)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
            //down-left
            else if (gridCell.Pos.x == _currentTile.Pos.x - 1 && gridCell.Pos.y == _currentTile.Pos.y - 1)
            {
                neighbors.Add(gridCell.OccupiedTile);
            }
        }
    }

    private void SetAngleToLine(Tile _currentTile, Tile _previousTile)
    {
        // rip performance >_<
        //left
        if (_currentTile.Pos.x < _previousTile.Pos.x && _currentTile.Pos.y == _previousTile.Pos.y)
        {
            _previousTile.ActivateLineLink(90);
        }
        //left-up
        if (_currentTile.Pos.x < _previousTile.Pos.x && _currentTile.Pos.y > _previousTile.Pos.y)
        {
            _previousTile.ActivateLineLink(45);
        }
        //up
        if (_currentTile.Pos.x == _previousTile.Pos.x && _currentTile.Pos.y > _previousTile.Pos.y)
        {
            _previousTile.ActivateLineLink(0);
        }
        //up-right
        if (_currentTile.Pos.x > _previousTile.Pos.x && _currentTile.Pos.y > _previousTile.Pos.y)
        {
            _previousTile.ActivateLineLink(-45);
        }
        //right
        if (_currentTile.Pos.x > _previousTile.Pos.x && _currentTile.Pos.y == _previousTile.Pos.y)
        {
            _previousTile.ActivateLineLink(-90);
        }
        //right-down
        if (_currentTile.Pos.x > _previousTile.Pos.x && _currentTile.Pos.y < _previousTile.Pos.y)
        {
            _previousTile.ActivateLineLink(-135);
        }
        //down
        if (_currentTile.Pos.x == _previousTile.Pos.x && _currentTile.Pos.y < _previousTile.Pos.y)
        {
            _previousTile.ActivateLineLink(-180);
        }
        //down-left
        if (_currentTile.Pos.x < _previousTile.Pos.x && _currentTile.Pos.y < _previousTile.Pos.y)
        {
            _previousTile.ActivateLineLink(135);
        }
    }
    #endregion PERFORMANCE KILLERS :(


    #region POWERUPS
    public List<int> SortTiles()
    {
        var itemList = (from t in tiles
                        select t).OrderByDescending(c => c.ValueId);
        List<int> intArray = new List<int>();
        foreach (var item in itemList)
        {
            intArray.Add(item.ValueId);
        }
        return intArray;
    }
    public void IncreaseMultiplier()
    {
        scoreMultiplier++;
    }
    #endregion POWERUPS


    #region GETTERS
    public List<GridCell> GetListOfFreeGridCells()
    {
        return grid.Where(n => n.OccupiedTile == null).ToList();
    }
    public int GetMergeValue()
    {
        if (connectedTiles.Count < 4)
        {
            return connectedTiles[0].ValueId + 1;
        }
        else if (connectedTiles.Count >= 4 && connectedTiles.Count < 8)
        {
            return connectedTiles[0].ValueId + 2;
        }
        else if (connectedTiles.Count >= 8 && connectedTiles.Count < 16)
        {
            return connectedTiles[0].ValueId + 3;
        }
        return MergeSumResult;
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
    public int GetValueFromGeometricSeries(int _id)
    {
        return gpSeries[_id];
    }
    #endregion GETTERS
}
