using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    // Variables

    private GameModel gameModel;
    private InputReader inputReader;

    private Coroutine coroutine;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {

            foreach (var item in gameModel.grid)
            {
                if (item.OccupiedTile != null)
                {
                    Debug.Log("Grid: Pos X: " + item.Pos.x + " Pos Y: " + item.Pos.y);
                    Debug.Log("Tile: Pos X: " + item.OccupiedTile.Pos.x + " Pos Y: " + item.OccupiedTile.Pos.y);
                    Debug.Log("//////////////////////////////////////////");
                }
                else
                {
                    Debug.Log("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                }
            }
        }
    }
    private void Awake()
    {
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
        gameModel.GenerateGrid(width, height);
        SpawnTilesOnEmptyGridCells(GetRandomIdValue(minTileValue, maxTileValue));
    }
    bool tempBool;
    private void StartTouch(Vector2 screenPosition, float time)
    {
        Debug.Log("Down!");
        var rayHit = CastRayOnMouse();
        if (rayHit && rayHit.collider.gameObject.TryGetComponent(out Tile _tile))
        {
            gameModel.TouchStarted(_tile);
            coroutine = StartCoroutine(TouchHold());
        }
        
    }

    private IEnumerator TouchHold()
    {
        yield return new WaitForSeconds(0.1f);
        while (true && gameModel.GetStartTile() != null)
        {
            var ray = CastRayOnMouse();
            if(ray && ray.collider.gameObject.TryGetComponent(out Tile _tile))
            {
                gameModel.CheckForPossibilities(_tile);
                //Debug.Log(gameModel.GetConnectedTiles().Count);
            }
            yield return null;
        }
    }

    private void EndTouch(Vector2 screenPosition, float time)
    {
        Debug.Log("UP!");
        if(coroutine!= null)
        {
            StopCoroutine(coroutine);
        }
        

        if(gameModel.CheckForConnectedTiles())
        {
            StartCoroutine(MergeWrapper());
            
        }
        gameModel.TouchEnded();
    }

    private IEnumerator MergeWrapper()
    {
        // Merge Tiles
        yield return StartCoroutine(gameModel.MergeTiles());
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Done1");
        var connectedTiles = gameModel.GetConnectedTiles();
        var mergeValue = gameModel.GetMergeValue();
        SpawnTile(connectedTiles.Last().GridCell, mergeValue);
        Debug.Log("Done2");
        yield return new WaitForSeconds(0.5f);
        // Make Tiles Fall
        yield return StartCoroutine(gameModel.MakeTilesFall());
        Debug.Log("Done3");
        yield return new WaitForSeconds(0.5f);
        //Spawn new Tiles
        SpawnTilesOnEmptyGridCells(GetRandomIdValue(minTileValue, maxTileValue));
        Debug.Log("Done4");
        yield return new WaitForSeconds(0.5f);
    }

    private void SpawnTilesOnEmptyGridCells(int _amount)
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
        var tile = Instantiate(tilePrefab, _gridCell.Pos, Quaternion.identity);
        gameModel.AddTile(tile, _gridCell, _index);
    }

    private int GetRandomIdValue(int min, int max)
    {
        int rnd = Random.Range(min, max);

        //Debug.Log(rnd);
        return rnd;
    }

    private RaycastHit2D CastRayOnMouse()
    {
        return Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay
                        (inputReader.touchControls.Player.TouchPosition.ReadValue<Vector2>()));
    }
}
