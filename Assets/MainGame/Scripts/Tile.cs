using DG.Tweening;
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
        //
        //pop up anim here ;D
    }
    
    public void SetTile(GridCell _gridCell)
    {
        if (GridCell != null) GridCell.OccupiedTile = null;
        GridCell = _gridCell;
        Pos = _gridCell.Pos;
        GridCell.OccupiedTile = this;
    }

    public void ActivateLineLink(int _angle)
    {
        lineLink.transform.rotation = Quaternion.Euler(0,0,_angle);
        lineLink.SetActive(true);
        Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
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
        
    }

    public void Contract()
    {

    }

    public void Obliterate(float _timer)
    {
        Destroy(gameObject, _timer);
    }
}
