
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetraminoData[] tetraminoes; //amount of shapes we have - 7 pieces we defined
    public GameObject tileCollider;
    public GameObject groundCollider;
    public GameObject player;
   

    private List<GameObject> pieceCollider = new List<GameObject> ();
    private List<GameObject> boardCollider = new List<GameObject>();
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
   
    private Vector3Int spawnPosition;
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
        
       
      
        Time.timeScale = 1f;
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
       
        for (int i = 0; i < this.tetraminoes.Length; i++)   // for every piece we go and initialize it with cell data
        {
            this.tetraminoes[i].Initialize();

        }

    }

    private void Start()
    {
        SpawnPiece();
        
    }


    #region Private methods


    /// <summary>
    /// Clears the line (row) on board and removes tiles and colliders, moves everything down to take the empty space
    /// </summary>
    /// <param name="row">the row that will be cleared</param>

    private void ClearBoardCollider()
    {
        for (int j = 0; j < boardCollider.Count; j++)
        {
            Destroy(boardCollider[j].gameObject);
        }

        boardCollider.Clear();
    }

    private void ClearPieceCollider()
    {
        for (int j = 0; j < pieceCollider.Count; j++)
        {
            Destroy(pieceCollider[j].gameObject);
        }
        pieceCollider.Clear();
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
                Vector3Int tilePosition2 = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(tilePosition2);

                tilePosition2 = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(tilePosition2, above);
            }
            row++;
        }
    }




    /// <summary>
    /// Checks if the row is full
    /// </summary>
    /// <param name="row">the row that;s being checked on board</param>
    /// <returns></returns>
    private bool IsLineFull(int row)
    {

        RectInt bounds = this.Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int tilePosition = new Vector3Int(col, row, 0);
            if (!this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    
    
    /// <summary>
    /// Restarts the game in 2 sec, clears the board and the colliders
    /// </summary>
    /// <returns></returns>
    IEnumerator RestartGame()
    {
        yield return new WaitForSecondsRealtime(2f);
        this.tilemap.ClearAllTiles();
        ClearBoardCollider();
        GameManager.IsInputEnabled = true;
        // SceneManager.LoadScene("Tetris");
    }

    #endregion



    #region Public methods
    public void SpawnPiece()
    {
        int xTempPlayer = 0;

        if (player.transform.position.x >0)
        xTempPlayer = Mathf.FloorToInt(player.transform.position.x);
        else if (player.transform.position.x < 0)
        xTempPlayer = Mathf.FloorToInt(player.transform.position.x);

        //we push the position a bit to the side to fit the tile

        //if (xTempPlayer < -3) xTempPlayer = -3;
        else if (xTempPlayer >3) xTempPlayer = 3;

        spawnPosition = new Vector3Int(xTempPlayer, 8, 0);
        

        int random = Random.Range(0, this.tetraminoes.Length);
        TetraminoData data = this.tetraminoes[random];
        //TetraminoData data = this.tetraminoes[0];
        this.activePiece.Initialize(this, spawnPosition, data);

        //for the long tile we have to push the position further
        
        switch (data.tetramino)
        {
            case Tetramino.I:
                {
                    if (xTempPlayer > 2) xTempPlayer = 2;
                    else if (xTempPlayer < -4) xTempPlayer = -4;

                    spawnPosition = new Vector3Int(xTempPlayer, 8, 0);
                    this.activePiece.Initialize(this, spawnPosition, data);
                }
                break;
        default: { break; }
        }

        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);
        }
        else
        {
            GameOver();
        }


    }



    public void UpdateBoardCollider()
    {

        ClearPieceCollider();
        ClearBoardCollider();
        RectInt bounds = this.Bounds;


        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            for (int row = bounds.yMin; row < bounds.yMax; row++)
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
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
            pieceCollider.Add(Instantiate(tileCollider, tilePosition, Quaternion.identity));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="piece"></param>

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
            ClearPieceCollider();
        }
    }


    

    /// <summary>
    /// Checks if the piece can be place on the passed position.
    /// If the piece is a ghost piece, it doesnt end the game when colliding with player
    /// </summary>
    /// <param name="piece"> piece to check</param>
    /// <param name="position">place on board we are checking</param>
    /// <param name="ghost">if the method is called by ghost piece</param>
    /// <returns></returns>
    public bool IsValidPosition(Piece piece, Vector3Int position, bool ghost = false)
    {

        RectInt bounds = this.Bounds;


        // The position is only valid if every cell is valid
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            Vector2 positionBoundsPlayer = new Vector2(tilePosition.x - 0.5f, tilePosition.y - 1.0f);
            Vector2 sizeBoundsPlayer = new Vector2(1.5f, 1f); //size is 2 but i want the player to fit on one tile
            Rect boundsPlayer = new Rect(positionBoundsPlayer, sizeBoundsPlayer);
            if (boundsPlayer.Contains((Vector2)player.transform.position))
            {
                if (!ghost)
                {
                    GameOver();

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

    /// <summary>
    /// Checks all rows (lines) and calls function to clear tit if yes
    /// </summary>
    public void ClearLines()
    {
        RectInt bounds = this.Bounds;

        int row = bounds.yMin;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
            }
            else
            {
                row++;
            }
        }


    }

    /// <summary>
    /// Stops the game by disable all input (and piece update function all together - no moving down) and restarting 
    /// the game
    /// </summary>
    public void GameOver()
    {
        GameManager.IsInputEnabled = false; //disable all inputs
        StartCoroutine(RestartGame());

    }


    #endregion
}
