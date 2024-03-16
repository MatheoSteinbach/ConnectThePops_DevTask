using UnityEngine;

public class GridCell 
{
    public Tile OccupiedTile;
    public Vector2 Pos;

    public GridCell (int _x, int _y)
    {
        Pos = new Vector2( _x, _y );
    }
    public GridCell(Vector2 pos)
    {
        Pos = pos;
    }
}
