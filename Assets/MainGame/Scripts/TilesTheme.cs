using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TilesTheme", menuName = "TileTheme")]
public class TilesTheme : ScriptableObject
{
    public Color backgroundColor;
    public bool showText = true;
    public List<TileStyle> tilesTheme;
}

[Serializable]
public struct TileStyle
{
    public Sprite sprite;
    public Color color;
}