using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Blink : MonoBehaviour
{
    
    public Board board;
    public Piece trackingPiece;

    public Tilemap tilemap { get; private set; }
    public Vector3Int position { get; private set; }
    public Vector3Int[] cells { get; private set; }


    private bool blinking;
    private Material materialTile;
  
    
   
    
    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.cells = new Vector3Int[Data.Cells[Tetramino.I].Length];

        materialTile = this.GetComponentInChildren<TilemapRenderer>().sharedMaterial;

        materialTile.SetFloat("_Glow", 2f);
        materialTile.SetFloat("_Render", 1f);
        materialTile.SetFloat("_Blink", 0f);

        blinking = false;

    }


   


    private void LateUpdate()
    {
        
        Clear();
        
        Copy();

        if (!trackingPiece.Blinking)
        {
            blinking = false;
        }


        //if the blinking is passed from the piece to be true (either for player of for drop, we render and blink

        if (trackingPiece.Blinking&&!blinking)
        {

            materialTile.SetFloat("_Render", 1f);
            materialTile.SetFloat("_Glow", 1f);
            StartCoroutine(BlinkTile());
            blinking = true;

        }

        //if we are not following the player and not blinking we dont render
        
        if (!trackingPiece.Blinking&&!trackingPiece.followPlayer)
        {
            
            materialTile.SetFloat("_Blink", 0f);
            materialTile.SetFloat("_Render", 0f);
           
        }

        //if we are not blinking but still following player, we glow

        if (!trackingPiece.Blinking && trackingPiece.followPlayer)
        {

            materialTile.SetFloat("_Blink", 0f);
            materialTile.SetFloat("_Render", 1f);
            materialTile.SetFloat("_Glow", 2f);

        }

      


        Set();
    }


    IEnumerator BlinkTile()
    {
        

        while (trackingPiece.Blinking)
       {
            yield return new WaitForSeconds(0.1f);

            materialTile.SetFloat("_Blink", 1f);

            yield return new WaitForSeconds(0.1f);

            materialTile.SetFloat("_Blink", 0f);
        }

    }


    private void Clear()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {

            this.cells[i] = this.trackingPiece.cells[i];
        }
    }

    

    private void Set()
    {
       

        this.position = trackingPiece.position;
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, trackingPiece.data.tile);
        }
    }

}