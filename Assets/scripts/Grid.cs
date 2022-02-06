using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Grid : MonoBehaviour
{
  public int width;
  public int height;
  //public UnityEngine.UI.RawImage gridImg;
  public Sprite gridImg;
  public string imgPathMultiBg;
  public string imgPathGhostHouse;
  public Tilemap tilemapWalls;
  public Tilemap tilemapGhostDoor;
  public RuleTile ruleTileRegWall;
  public RuleTile ruleTileGhostHouse;
  public RuleTile ruleTileGhostDoor;

  private MazeTileTypes mazeTileTypes;
  private MazeTileTypes mazeGhostHouseTileTypes;

  public readonly Vector2Int[] directions = {
    new Vector2Int(0,1),  // up
    new Vector2Int(1,0),  // right
    new Vector2Int(0,-1), // down
    new Vector2Int(-1,0)  // left
  };

  // TODO - maybe move to interface class Movement? or utility movement class?
  public enum Dir
  {
    None = -1,
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3,
    Size = 4
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

    // TODO - add comments
    float edge = (float) (numPixelsPerTile - 1) / pixelateFactor;
    targetPosOnTile = new Vector2[4] {
      new Vector2(0.5f, 0.0f),  // entering tile from below
      new Vector2(0.0f, 0.5f),  // entering tile from left
      new Vector2(0.5f, edge),  // entering tile from above
      new Vector2(edge, 0.5f)   // entering tile from right
    };

    // create model for the maze tile types
    mazeTileTypes = new MazeTileTypes(imgPathMultiBg, width, height);
    ghostHouseTileTypes = new MazeTileTypes(imgPathGhostHouse, width, height);

    PlaceTiles(ref mazeTileTypes);
    PlaceTiles(ref ghostHouseTileTypes);
  }


// ----------------------------------------------------------------------------
// --------------- tile and position utility methods ------------------------
// ----------------------------------------------------------------------------
  public Vector2Int GetTileCoordinate(Vector2 pos)
  {
    // add 1 x and y offset to take the border into account
    return new Vector2Int((int) pos.x, (int) pos.y);
  }

  // returns target position given targetTile and enter Direction
  public Vector2 GetMoveToPos(Vector2Int targetTile, Dir enterDir)
  {
    Vector2 dirVector = targetPosOnTile[(int) enterDir];
    // todo - can we simplify this, is dirVector a ref or copy?
    return new Vector2(dirVector.x + targetTile.x, dirVector.y + targetTile.y);
  }

  // returns a vector2 with the position of the tile center
  public Vector2 GetCenterPos(Vector2Int tile) {
    return new Vector2(0.5f + tile.x, 0.5f + tile.y);
  }


// ----------------------------------------------------------------------------
// --------------- Pixel utility methods -------------------------
// ----------------------------------------------------------------------------
  // returns a the coordinate of the pixel that corresponds to the position
  // e.g. 8 pixels per tile --> floor(pos * 8)
  public Vector2Int PixelCoordinate(Vector2 pos)
  {
    // blow up Vector2 to pixel level and take floor value
    pos = pos * pixelateFactor;
    Vector2Int pixelCoordinate = Vector2Int.FloorToInt(pos);
    return pixelCoordinate;
  }

  // returns a the position that corresponds to the pixel coordinate
  // e.g. 8 pixels per tile --> pixelCoordinate to Vector2 and divide by 8
  public Vector2 Position(Vector2Int pixelCoordinate) {
    // convert Vector2Int to regular Vector2 and shrink again
    Vector2 pos = (Vector2)pixelCoordinate;
    pos = pos * invPixelateFactor;
    return pos;
  }

  // returns a the position snapped to the pixel size
  // for example e.g. 8 pixels per tile --> snap to 0.125 values
  public Vector2 SnapToPixel(Vector2 pos)
  {
    // pixelate position: blow up, apply floor and shrink down again
    Vector2Int pixelCoordinate = PixelCoordinate(pos);
    return Position(pixelCoordinate);
  }


// ----------------------------------------------------------------------------
// --------------- tile and direction utility methods -------------------------
// ----------------------------------------------------------------------------
  // returns the to the currentTile adjacent tile based on the th
  public Vector2Int GetAdjacentTile(Vector2Int currentTile, Dir dir) {
    // NOTE: currentTile is a copy, not a reference to the passed Coordinate
    Vector2Int tileDir = directions[(int)dir];
    return currentTile + tileDir;
  }

  // returns the direction based on two adjacent tiles
  public Dir GetDirectionAdjacentTiles(Vector2Int tile1, Vector2Int tile2) {
    Vector2 tileDelta = tile2 - tile1;
    // up
    if(tileDelta.Equals(directions[0])) return Dir.Up;
    // Right
    if(tileDelta.Equals(directions[1])) return Dir.Right;
    // down
    if(tileDelta.Equals(directions[2])) return Dir.Down;
    // left
    if(tileDelta.Equals(directions[3])) return Dir.Left;

    throw new System.Exception("Grid.GetDirection - direction = none");
  }



// ----------------------------------------------------------------------------
// --------------- tile validity utility methods ------------------------------
// ----------------------------------------------------------------------------
  public bool TileIsPath(Vector2Int currentTile) {
    return mazeTileTypes.TileIsPath(currentTile);
  }

  public bool GhostMoveUpForbidden(Vector2Int currentTile) {
    MazeTileTypes.TileID tileID = mazeTileTypes.GetTileID(currentTile);
    return tileID == MazeTileTypes.TileID.NoUpward;
  }

  public bool WallIsGhostDoor(Vector2Int currentTile) {
    MazeTileTypes.TileID tileID = mazeTileTypes.GetTileID(currentTile);
    return tileID == MazeTileTypes.TileID.GhostDoor;
  }


// ----------------------------------------------------------------------------
// --------------- dinstance utility methods -----------------------------------
// ----------------------------------------------------------------------------

  // distance
  public int SquaredEuclideanDistance(Vector2Int tile1, Vector2Int tile2) {
    int a = tile2.x - tile1.x;
    int b = tile2.y - tile1.y;
    return a * a + b * b;
  }

  public int ManhattanDistance(Vector2Int tile1, Vector2Int tile2) {
    int a = tile2.x - tile1.x;
    int b = tile2.y - tile1.y;
    return System.Math.Abs(a) + System.Math.Abs(b);
  }


// ----------------------------------------------------------------------------
// --------------- Tile map method -------------------------------------------
// ----------------------------------------------------------------------------


  private void PlaceTiles(ref MazeTileTypes tileTypes)
  {
    //Tilemap tmap = GetComponent<Tilemap>();
    //TileBase tile = (TileBase) TileBase.CreateInstance("Assets/tiles/walls-inner.asset");// tilemapWalls.GetTile(new Vector3Int(0, 31, 0));

    // also draw a border with rule tiles, to ensure correct display of border
    int size = width * height;
    for(int i = 0; i < size; i++) {
      Vector2Int tileCoord = new Vector2Int(0,0);
      tileCoord.y = i / width;
      tileCoord.x = i - (tileCoord.y * width); // * is cheaper than %
      Vector3Int pos = new Vector3Int(tileCoord.x, tileCoord.y, 0);
      // if tile coordinate is wall - add wall tile
      if(!tileTypes.TileIsPath(tileCoord)) {
        // place the corresponding wall tile
        switch(tileTypes.GetTileID(tileCoord))
        {
          case MazeTileTypes.TileID.RegWall:
            tilemapWalls.SetTile(pos, ruleTileRegWall);
            break;
          case MazeTileTypes.TileID.GhostHouse:
            tilemapWalls.SetTile(pos, ruleTileGhostHouse);
            break;
          case MazeTileTypes.TileID.GhostDoor:
            // TODO - position the door based on a seperate image,
            //        so you can take the orientation into account
            tilemapGhostDoor.SetTile(pos, ruleTileGhostDoor);
            break;
          default:
            // default - do nothing
            break;
        }
      }
    }
  }


}
