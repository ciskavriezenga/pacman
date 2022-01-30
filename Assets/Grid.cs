using System
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TilePos
{
  public int x { get; set; }
  public int y { get; set; }
  public add(TilePos pos) {
    x = x + pos.x;
    y = y + pos.y;
  }
}

public class Grid : MonoBehaviour
{
  public int width;
  public int height;
  public Texture2D img;


  private TilePos[] directions {
    new TilePos(0,1),
    new TilePos(1,0),
    new TilePos(0,-1),
    new TilePos(-1,0)
  }

  private TilePos[] targetPosRelative {
    // TODO - was here
    new Vector2d(0, )
    new TilePos(1,0),
    new TilePos(0,-1),
    new TilePos(-1,0)
  }

  private Vector2 worldFactor = 8.0f;
  private Vector2 toWorldFactor = new Vector2(worldFactor, worldFactor);
  private Vector2 toGridFactor = new Vector2(1.0f / worldFactor, 1.0f / worldFactor);

  public TileCoordinate GetTileCoordinate(Vector2 pos)
  {
    return new TilePos((int) pos.x, (int) pos.y);
  }

  public GetTargetPos(TilePos targetTile, Dir enterDir)
  {
    // return target position given targetTile and enter Direction

  }

  public GetTileCenterPos(TilePos tile) {
    return new Vector2((float)tile.x + 0.5f, (float)tile.y + 0.5f);
  }



  public GetTargetTile(TilePos currentTile, Dir dir) {
    TilePos dirVector = directions[currentDir];
    return currentTile.add(dirVector);
  }

  public bool TileIsPath(TilePos currentTile) {
    return true;
    // TODO fix and use below
    int imgIndex = currentTile.x + currentTile.y * width;
    Color[] pixels = img.GetPixels();
    if(pixels[imgIndex]  == Color.white) // Color.black
  }

  public Vector2 SnapToPixel(Vector2 pos)
  {
   // copy to new Vector2
   Vector2 pixelPos = new Vector2(pos);
   // blow up to world and appply floor
   pixelPos.x = MathF.Floor(pixelPos.x * worldFactor);
   pixelPos.y = MathF.Floor(worldPos.y * worldFactor);

   // shrink down again
   return pixelPos * toGridFactor;
  }


}
