using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof (SpriteRenderer))]
public class Tile : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private GameObject lineLink;

    private SpriteRenderer spriteRenderer;

    public int ValueId;
    public int Value;
    [HideInInspector] public GridCell GridCell;
    [HideInInspector] public Vector2 Pos;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Init(TileStyle _style, bool _showText, int _value, int _valueId)
    {
        gameObject.transform.position = Pos;
        gameObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        gameObject.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
        Value = _value;
        ValueId = _valueId;

        lineLink.SetActive(false);
        spriteRenderer.sprite = _style.sprite;
        spriteRenderer.color = _style.color;
        lineLink.GetComponent<SpriteRenderer>().color = _style.color;
        name = "Tile (" + Pos.x + "," + Pos.y + ")";
        if (_showText)
        {
            text.text = Value.ToString();
        }
        else
        {
            text.text = "";
        }
    }
    
    public void SetTile(GridCell _gridCell)
    {
        GridCell = _gridCell;
        Pos = _gridCell.Pos;
        
        GridCell.OccupiedTile = this;
    }
    public void ChangeTheme(TilesTheme _theme)
    {
        var style = _theme.tilesTheme[ValueId];
        spriteRenderer.sprite = style.sprite;
        spriteRenderer.color = style.color;
        lineLink.GetComponent<SpriteRenderer>().color = style.color;
        if (_theme.showText)
        {
            text.text = Value.ToString();
        }
        else
        {
            text.text = "";
        }
    }

    public void ActivateLineLink(int _angle)
    {
        lineLink.transform.rotation = Quaternion.Euler(0,0,_angle);
        lineLink.SetActive(true);
    }

    public void DeactivateLineLink()
    {
        lineLink.SetActive(false);
    }

    public void MoveTo(Vector2 _pos, float _speed)
    {
        gameObject.transform.DOLocalMove(_pos, _speed);
    }

    public void Expand()
    {
        gameObject.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.5f);
    }

    public void Contract()
    {
        gameObject.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.5f);
    }

    public void Obliterate()
    {
        StartCoroutine(BackToThePool());
    }
    private IEnumerator BackToThePool()
    {
        yield return new WaitForSeconds(0.3f);

        GridCell.OccupiedTile = null;
        GameController.Instance.TilePool.Release(gameObject.GetComponent<Tile>());
    }
}
