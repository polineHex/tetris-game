
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetraminoData[] tetraminoes; //amount of shapes we have - 7 pieces we defined
    public GameObject tileCollider;
    public GameObject groundCollider;
    public GameObject player;
    private bool isDead = false;


    private List<GameObject> pieceCollider = new List<GameObject> ();
    private List<GameObject> boardCollider = new List<GameObject>();
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
   
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public RectInt Bounds
    {
        get { 
            Vector2Int cornerPosition = new Vector2Int(-this.boardSize.x/2, -this.boardSize.y/2);
            return new RectInt(cornerPosition, this.boardSize);
        }
       
    }


    private void Awake()
    {
        
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
        this.isDead = this.player.GetComponent<PlayerMovement>().IsDead;
        for (int i = 0; i < this.tetraminoes.Length; i++)   // for every piece we go and initialize it with cell data
        {
            this.tetraminoes[i].Initialize();

        }

    }

    private void Start()
    {
        SpawnPiece();
        
    }


    public void SpawnPiece()
    {
        
        int random = Random.Range(0, this.tetraminoes.Length);
        TetraminoData data = this.tetraminoes[random];
        this.activePiece.Initialize(this, spawnPosition, data);
        
        if (IsValidPosition(this.activePiece, this.spawnPosition)) {
            Set(this.activePiece);
        }
        else {
            GameOver();
        }
        
        
    }



    public void ClearBoardCollider()
    {
        for (int j = 0; j < boardCollider.Count; j++)
        {
            Destroy(boardCollider[j].gameObject);
        }

        boardCollider.Clear();
    }

    public void ClearPieceCollider()
    {
        for (int j = 0; j < pieceCollider.Count; j++)
        {
            Destroy(pieceCollider[j].gameObject);
        }
        pieceCollider.Clear();
    }
    public void UpdateBoardCollider()
    {

        ClearPieceCollider();
        ClearBoardCollider();
       RectInt bounds = this.Bounds;
       
        
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        { for (int row = bounds.yMin; row < bounds.yMax; row++)
                {
                Vector3Int tilePosition = new Vector3Int(col, row, 0);
                    if (this.tilemap.HasTile(tilePosition))
                    {
                        boardCollider.Add(Instantiate(groundCollider, tilePosition, Quaternion.identity));
                    }
                }
            }
   
    }
    public void Set(Piece piece)
    {
        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
          pieceCollider.Add(Instantiate(tileCollider, tilePosition, Quaternion.identity));
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition,null);
            ClearPieceCollider();
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position, bool ghost = false)
    {

        RectInt bounds = this.Bounds;
        

        // The position is only valid if every cell is valid
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;
            
            Vector2 positionBoundsPlayer = new Vector2(tilePosition.x - 0.5f, tilePosition.y - 1.0f);
            Vector2 sizeBoundsPlayer = new Vector2(2f,1f);
            Rect boundsPlayer = new Rect(positionBoundsPlayer,sizeBoundsPlayer);
            if(boundsPlayer.Contains((Vector2)player.transform.position))
            {
                if (!ghost)
                {
                    GameOver();
                    player.GetComponent<PlayerMovement>().IsDead = true;
                }
                return false;
            }

            // An out of bounds tile is invalid
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            // A tile already occupies the position, thus invalid
            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }

        }

        return true;
    }


    public void ClearLines()
    {
        RectInt bounds = this.Bounds;

        int row = bounds.yMin;

        while (row<bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
            }else {
                row++;
            }
        }
        
       
    }

    
    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int tilePosition = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(tilePosition, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int tilePosition2 = new Vector3Int(col, row+1, 0);
                TileBase above = this.tilemap.GetTile(tilePosition2);
                
                tilePosition2 = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(tilePosition2, above);
            }
            row++;
        }
    }
    
    private bool IsLineFull(int row)
    {

        RectInt bounds = this.Bounds;
        for (int col = bounds.xMin; col< bounds.xMax;  col++)
        {
            Vector3Int tilePosition = new Vector3Int(col,row, 0);
            if (!this.tilemap.HasTile(tilePosition)) {
                return false;
            }
        }
        return true;
    }

    private void GameOver()
    {
        this.tilemap.ClearAllTiles();
        ClearBoardCollider();
    }
}
