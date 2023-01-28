using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetraminoData data { get; private set; }
    public Vector3Int position { get; private set; }
    public Vector3Int[] cells { get; private set; }

    //we need to store rotation index
    public int rotationIndex { get; private set; }

    //time and levels of difficulty
    public float stepDelay = 1f;   //time it takes the tile to go down 
    public float lockDelay = 0.5f;
    public float playerLockDelay = 5f;
    public float timeToDecide = 6f; // time that the tile is sticking to the player

    private bool followPlayer;
    private float timeToDecideSum;
    private float stepTime;
    private float lockTime;
    
    //private Material materialTile;
   
    public float TimeToDecideSum
    {
        get { return timeToDecideSum; }
    }


    //filling the cells with data from Data script dictionary - basically filling out the form for the picked piece    
    public void Initialize(Board board, Vector3Int position, TetraminoData data)
    {
      this.board = board;
        
      //materialTile = board.GetComponentInChildren<TilemapRenderer>().sharedMaterial;
      //materialTile.SetFloat("_Glow", 2f);

        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + stepDelay; //current game time + step, so first step will happen after 1second on first level of difficulty
        this.lockTime = 0f;

        timeToDecideSum =timeToDecide+Time.time; //if we start at 0 sec, we now have tile 0+timeToDecide to move the tile together with player
        followPlayer = true;

        this.gameObject.layer = LayerMask.NameToLayer("Ground");

        

    if (this.cells ==null){
            this.cells = new Vector3Int[this.data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++){
            this.cells[i] = (Vector3Int)data.cells[i];

        }


    }
    //reading input and updating the tiles
    
    //until timeToDecide the tile is stuck to the player, same controls control the player and the tile
    //when Time.time is > timeToDecide, the tile cant move right or left, but it can rotate - we block input from left/right
    
    private void Update()
    {
        if (GameManager.IsInputEnabled)
        {
            this.board.Clear(this);
            this.lockTime += Time.deltaTime;

            //rotation 
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Rotate(-1);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                Rotate(1);
            }

            //moving left/right - with player only
            if (followPlayer)
            {
                if (Input.GetKeyDown(KeyCode.A)) /// change the key event!!!!!!!!!!!!
                {
                    Move(Vector2Int.left);
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    Move(Vector2Int.right);
                }
            }
            
            //moving down/drop down - we allow always?
            if (Input.GetKeyDown(KeyCode.S))
            {
                Move(Vector2Int.down);
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                HardDrop();
            }

            if(Time.time > timeToDecideSum)
            {
                followPlayer= false;
                //materialTile.SetFloat("_Glow", 1f);
               
            }

           

            if (Time.time >= this.stepTime)
            {
                Step();
            }
            this.board.Set(this);
        }
       
    }


   
    
    
    private void Step()
    {
        this.stepTime = Time.time + stepDelay;

        // Step down to the next row
        Move(Vector2Int.down);

        // Once the piece has been inactive for too long it becomes locked
        if (this.lockTime >= this.lockDelay)
        {
            Lock();
        }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    private void Lock()
    {
        board.Set(this);
       
        board.ClearLines();

        board.UpdateBoardCollider();
        board.SpawnPiece();
    }
  

    //check bounds and if there is another piece, if not - updates position
    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        // Only save the movement if the new position is valid
        if (valid)
        {
            this.position = newPosition;
            //moveTime = Time.time + moveDelay;
            this.lockTime = 0f; // resets inactivae time
        }

        return valid;
    }


    private void Rotate(int direction)
    {
        // Store the current rotation in case the rotation fails
        // and we need to revert
        int originalRotation = rotationIndex;

        // Rotate all of the cells using a rotation matrix
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        
        
        // Revert the rotation if the wall kick tests fail
        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        // Rotate all of the cells using the rotation matrix
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (this.data.tetramino)
            {
                case Tetramino.I:
                case Tetramino.O:
                    // "I" and "O" are rotated from an offset center point
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

         for (int i = 0; i < data.wallKicks.GetLength(1); i++)
         {
             Vector2Int translation = data.wallKicks[wallKickIndex, i];

             if (Move(translation))
             {
                 
                 return true;
             }
         }

      
        return false;
       // return board.IsValidPosition(this, position); - no wallkick, just checks if position is valid
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }


}
