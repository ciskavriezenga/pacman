using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
  public int width;
  public int height;
  //public UnityEngine.UI.RawImage gridImg;
  public Sprite gridImg;
  public string imgPath = "./assets/pacman.png";
  private Color[] gridPixels;

  private TileCoordinate[] directions = {
    new TileCoordinate(0,1),  // up
    new TileCoordinate(1,0),  // right
    new TileCoordinate(0,-1), // down
    new TileCoordinate(-1,0)  // left
  };

  // TODO - maybe move to interface class Movement? or utility movement class?
  public enum Dir
  {
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
  }

  private Vector2[] targetPosOnTile;
  // values for the process of snapping the position to a pixel
  private int numPixelsPerTile = 8;
  private float pixelateFactor = 0;
  private float invPixelateFactor = 0;

  void Awake()
  {
    // values for the process of snapping the position to a pixel
    pixelateFactor = (float) numPixelsPerTile;
    invPixelateFactor = 1.0f / pixelateFactor;

    // TODO - move to pacman movement? -
    //        this also will depend on the implementation of the ghost movement
    // targetPosOnTile is used to set the target position based on how pacman
    // moves to the target tile
    float edge = (float) (numPixelsPerTile - 1) / pixelateFactor;
    targetPosOnTile = new Vector2[4] {
      new Vector2(0.5f, 0.0f),  // entering tile from below
      new Vector2(0.0f, 0.5f),  // entering tile from left
      new Vector2(0.5f, edge),  // entering tile from above
      new Vector2(edge, 0.5f)   // entering tile from right
    };

    // TODO - add safety check if file exists
    byte[] imgData = System.IO.File.ReadAllBytes(imgPath);
    Texture2D gridTexture2D = new Texture2D(width, height);
    gridTexture2D.LoadImage(imgData);
    gridPixels = gridTexture2D.GetPixels();
  }


  public TileCoordinate GetTileCoordinate(Vector2 pos)
  {
    return new TileCoordinate((int) pos.x, (int) pos.y);
  }

  public TileCoordinate GetTargetTile(TileCoordinate currentTile, Dir dir) {
    TileCoordinate tileDir = directions[(int)dir];
    currentTile.Add(tileDir);
    return currentTile;
  }

  public Vector2 GetTargetPos(TileCoordinate targetTile, Dir enterDir)
  {
    // return target position given targetTile and enter Direction
    return targetTile.Add(targetPosOnTile[(int) enterDir]);
  }

  public Vector2 GetTileCenterPos(TileCoordinate tile) {
    return tile.Add(0.5f, 0.5f);
  }

  public bool TileIsPath(TileCoordinate currentTile) {
    int gridPixelsIndex = currentTile.x + currentTile.y * width;
    bool isPath = gridPixels[gridPixelsIndex]  == Color.white; // Color.black
    Debug.Log("Tile is path: " + isPath);
    return isPath;
  }

  public Vector2 SnapToPixel(Vector2 pos)
  {
    // NOTE: pos is a copy, not a reference to the passed position
    // pixelate position: blow up, apply floor and shrink down again
    pos.x = System.MathF.Floor(pos.x * pixelateFactor);
    pos.y = System.MathF.Floor(pos.y * pixelateFactor);
    return pos * invPixelateFactor;
  }


}
