using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;


public class GameController : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 5;
    [Header("Prefabs")]
    [SerializeField] private GridCell gridPrefab;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [Header("Themes")]
    [SerializeField] private List<TilesTheme> themeList;
    [SerializeField] private bool useGeometricSeries = true;
    [Header("Tiles")]
    [SerializeField] int minTileValue = 0;
    [SerializeField] int maxTileValue = 5;
    [Header("Speeds")]
    [SerializeField] float mergeSpeed = 0.2f;
    [SerializeField] float fallSpeed = 0.25f;
    [SerializeField] float respawnDelay = 0.15f;
    [Header("Preview")]
    [SerializeField] Tile previewTile;
    [SerializeField] UITexts uiTexts;
    // Variables
    [HideInInspector] public ObjectPool<Tile> TilePool;

    private GameModel gameModel;
    private InputReader inputReader;
    private Coroutine pressedDownCoroutine;
    private bool hasFinished = true;

    // Wasn't planned for this class to be a singleton, but I needed reference to the pool >_<
    public static GameController Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        gameModel = new GameModel();
        gameModel.SetCurrentTheme(themeList[0]);
        gameModel.SetUpValueNumbers(useGeometricSeries, 20);
        inputReader = InputReader.Instance;
    }
    private void OnEnable()
    {
        inputReader.OnStartTouch += StartTouch;
        inputReader.OnEndTouch += EndTouch;
    }
    private void OnDisable()
    {
        inputReader.OnStartTouch -= StartTouch;
        inputReader.OnEndTouch -= EndTouch;
    }

    private void Start()
    {
        // Setting up the Pooling
        TilePool = new ObjectPool<Tile>(() =>
        {
            return Instantiate(tilePrefab);
        }, tile =>
        {
            tile.gameObject.SetActive(true);
        }, tile =>
        {
            tile.gameObject.SetActive(false);
        }, tile =>
        {
            Destroy(tile.gameObject);
        }, true, 30, 40);


        gameModel.GenerateGrid(width, height);
        SpawnTilesOnEmptyGridCells();
    }

    private void StartTouch(Vector2 _screenPosition, float _time)
    {
        var rayHit = CastRayOnTouch();
        if (rayHit && rayHit.collider.gameObject.TryGetComponent(out Tile _tile))
        {
            gameModel.TouchStarted(_tile);

            pressedDownCoroutine = StartCoroutine(DragTouch());
        }
    }

    private IEnumerator DragTouch()
    {
        yield return new WaitForSeconds(0.1f);
        while (true && gameModel.GetStartTile() != null)
        {
            // On Tile Hit -> look for Connections in gameModel
            var ray = CastRayOnTouch();
            if (ray && ray.collider.gameObject.TryGetComponent(out Tile _tile))
            {
                gameModel.TouchIsHeld(_tile);     
            }
            // Preview Tile Setup
            if(gameModel.GetConnectedTiles().Count > 1) 
            {
                var id = gameModel.GetMergeValue();
                previewTile.gameObject.SetActive(true);
                previewTile.Value = gameModel.GetValueFromGeometricSeries(id);
                previewTile.ValueId = id;
                previewTile.ChangeTheme(gameModel.CurrentTheme);
            }
            else
            {
                var id = gameModel.GetStartTile().ValueId;
                previewTile.gameObject.SetActive(true);
                previewTile.Value = gameModel.GetValueFromGeometricSeries(id);
                previewTile.ValueId = id;
                previewTile.ChangeTheme(gameModel.CurrentTheme);
            }
            yield return null;
        }
    }
    
    private void EndTouch(Vector2 _screenPosition, float _time)
    {
        if (pressedDownCoroutine != null)
        {
            StopCoroutine(pressedDownCoroutine);
        }

        gameModel.TouchEnded();

        if (gameModel.Merging && hasFinished)
        {
            StartCoroutine(MergeWrapper());
            hasFinished = false;
        }
        previewTile.gameObject.SetActive(false);
    }

    private IEnumerator MergeWrapper()
    {
        // Merge Tiles
        yield return StartCoroutine(MergeTiles());
        yield return new WaitForSeconds(mergeSpeed);
        // Make Tiles Fall
        yield return StartCoroutine(gameModel.MakeTilesFall(fallSpeed));
        yield return new WaitForSeconds(respawnDelay);
        // Spawn new Tiles
        SpawnTilesOnEmptyGridCells();
        yield return new WaitForSeconds(0.22f);
        hasFinished = true;
    }
    private IEnumerator MergeTiles()
    {
        yield return StartCoroutine(gameModel.MergeTiles());
        yield return new WaitForSeconds(mergeSpeed + 0.1f);
        SpawnTile(gameModel.MergedGridCell, gameModel.GetMergeValue());
        AudioManager.Instance.MergeSound();
        gameModel.FinishedMerging();
        UpdateUI();
    }


    private void SpawnTilesOnEmptyGridCells()
    {
        var freeGridCells = gameModel.GetListOfFreeGridCells();
        for (int i = 0; i < freeGridCells.Count; i++)
        {
            var gridCell = freeGridCells[i];
            SpawnTile(gridCell, GetRandomIdValue(minTileValue, maxTileValue));
        }
    }

    private void SpawnTile(GridCell _gridCell, int _index)
    {
        var tile =  TilePool.Get();
        gameModel.AddTile(tile, _gridCell, _index);
    }

    private int GetRandomIdValue(int _min, int _max)
    {
        int rnd = Random.Range(_min, _max);
        return rnd;
    }

    private RaycastHit2D CastRayOnTouch()
    {
        return Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay
                        (inputReader.touchControls.Player.TouchPosition.ReadValue<Vector2>()));
    }

    public void ChangeThemeTo(int _id)
    {
        gameModel.SetCurrentTheme(themeList[_id]);
    }
    // Update UI -> normally done with events
    public void UpdateUI()
    {
        uiTexts.UpdateUI(gameModel.ScorePoints, gameModel.ScoreMultiplier, gameModel.CurrentLevel, gameModel.Progress);
    }

    #region POWERUPS
    public void SortTiles()
    {
        var tempList = gameModel.SortTiles();
        for (int i = 0; i < gameModel.grid.Count; i++)
        {
            var x = tempList[i];
            gameModel.grid[i].OccupiedTile.Init(gameModel.CurrentTheme.tilesTheme[x], true, gameModel.GetValueFromGeometricSeries(x), x);
        }
    }

    public void SpawnHigherNumbers()
    {
        minTileValue++;
        maxTileValue++;
    }

    public void DoubleValueOnTiles()
    {
        for (int i = 0; i < gameModel.grid.Count; i++)
        {
            var x = gameModel.grid[i].OccupiedTile.ValueId + 1;
            gameModel.grid[i].OccupiedTile.Init(gameModel.CurrentTheme.tilesTheme[x], true, gameModel.GetValueFromGeometricSeries(x), x);
        }
    }

    public void IncreaseMultiplier()
    {
        gameModel.IncreaseMultiplier();
        UpdateUI();
    }
    #endregion POWERUPS
}
