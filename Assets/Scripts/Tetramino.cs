using UnityEngine.Tilemaps;
using UnityEngine;
public enum Tetramino
{
    I,
    O,
    T,
    J,
    L,
    S,
    Z,
}

[System.Serializable]
public struct TetraminoData
{
    public Tetramino tetramino;
    public Tile tile;
    public Vector2Int[] cells; // { get; private set; }

    public Vector2Int[,] wallKicks { get; private set; }//wall kick data
    public void Initialize()
    {
        this.cells = Data.Cells[this.tetramino];
        this.wallKicks = Data.WallKicks[this.tetramino];
    }



}