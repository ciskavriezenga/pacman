using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PM {

public enum Dir
{
  NONE = -1,
  RIGHT = 0,
  DOWN = 1,
  LEFT = 2,
  UP = 3,
  SIZE = 4
}

public class Maze : MonoBehaviour
{
  // fields set in editor
  public Tilemap tilemapWalls;
  public Tilemap tilemapGhostDoor;
  public RuleTile ruleTileRegWall;
  public RuleTile ruleTileGhostHouse;
  public RuleTile ruleTileGhostDoor;

  // private settings
  public int width {get; private set;}
  public int height {get; private set;}
  // TODO - fix this to not hardcoded
  public int borderSize {get; private set;} = 2;

  // maze tile types - paths, ghost zones, pellets, ghosthouse rule tiles
  private MazeTileTypes pathsTileTypes;
  private MazeTileTypes ghostZonesTileTypes;
  private MazeTileTypes pelletsTileTypes;
  private MazeTileTypes ghosthouseTileTypes;
  public Vector2Int ghostDoorTargetTile {get; private set;}

  // TODO - replace met standard directions unity?
  public readonly Vector2Int[] directions = {
    new Vector2Int(1,0),  // right
    new Vector2Int(0,-1), // down
    new Vector2Int(-1,0),  // left
    new Vector2Int(0,1)  // up
  };


  // values for the process of snapping the position to a pixel
  private int numPixelsPerTile = 8;
  private float pixelateFactor;
  private float invPixelateFactor;
  // pixel positions for tiles, based on the direction of ghost / pacman
  private Vector2[] targetPosOnTile;


  public void Initialize(MazeSettings settings)
  {
    Debug.Log("MAZE - INITIALIZE");
    width = settings.width;
    height = settings.height;


    // factors used for the process of snapping the position to a pixel
    pixelateFactor = (float) numPixelsPerTile;
    invPixelateFactor = 1.0f / pixelateFactor;

    // pixel positions for tiles, based on the direction of ghost / pacman
    float edge = (float) (numPixelsPerTile - 1) / pixelateFactor;
    targetPosOnTile = new Vector2[4] {
      new Vector2(0.0f, 0.5f),  // entering tile from left
      new Vector2(0.5f, edge),  // entering tile from above
      new Vector2(edge, 0.5f),   // entering tile from right
      new Vector2(0.5f, 0.0f)  // entering tile from below
    };

    // create MazeTileTypes - models that hold the maze tile types
    pathsTileTypes = new MazeTileTypes(settings.imgMazePath, width, height);
    ghostZonesTileTypes = new MazeTileTypes(settings.imgMazeGhostZones, width, height);
    pelletsTileTypes = new MazeTileTypes(settings.imgMazePellets, width, height);
    ghosthouseTileTypes = new MazeTileTypes(settings.imgGhostHouseTiles, width, height);

    // TODO - fetch door as vector2 - store as vector2int + get offset.
    ghostDoorTargetTile = ghostZonesTileTypes.GetGhostDoorTargetTile();
    Debug.Log("Maze.Initialize - ghostDoorTargetTile " + ghostDoorTargetTile);
    // draw the tiles to the tilemap, according to the tiletype models
    DrawTiles(pathsTileTypes);
    DrawTiles(ghosthouseTileTypes);
    DrawTiles(ghostZonesTileTypes);
  }

  public void Awake()
  {
    Debug.Log("MAZE - AWAKE");
  }

  public void Start() {
    Debug.Log("MAZE - START");
  }


// =============================================================================
// =============== tile and position utility methods ===========================
// =============================================================================
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
  public Vector2 GetCenterPos(Vector2Int tile)
  {
    return new Vector2(0.5f + tile.x, 0.5f + tile.y);
  }


// ==============================================================================
// =============== Pixel utility methods ========================-
// ==============================================================================
  public Vector2Int GetCenterPixelPos(Vector2Int tile)
  {
    return PixelCoordinate(GetCenterPos(tile));
  }
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


// ==============================================================================
// =============== tile and direction utility methods ========================-
// ==============================================================================
  // returns the to the currentTile adjacent tile based on the th
  public Vector2Int GetAdjacentTile(Vector2Int currentTile, Dir dir) {
    // NOTE: currentTile is a copy, not a reference to the passed Coordinate
    Vector2Int tileDir = directions[(int)dir];
    return currentTile + tileDir;
  }


  public Vector2Int GetTileInDirection(Vector2Int currentTile, Dir dir,
    int numTilesAway, bool addBugOffset = false) {
    // NOTE: currentTile is a copy, not a reference to the passed Coordinate
    Vector2Int tileDir = directions[(int)dir];
    tileDir = tileDir * numTilesAway;

    // add overflow bug - additional offset to left if dir == UP
    if(addBugOffset && dir == Dir.UP) {
      // recursive call to add error offset to the left
      return GetTileInDirection(currentTile + tileDir, Dir.LEFT, numTilesAway);
    }

    return currentTile + tileDir;
  }

  // returns the direction based on two adjacent tiles
  public Dir GetDirectionAdjacentTiles(Vector2Int tile1, Vector2Int tile2) {
    Vector2 tileDelta = tile2 - tile1;
    // Right
    if(tileDelta.Equals(directions[0])) return Dir.RIGHT;
    // down
    if(tileDelta.Equals(directions[1])) return Dir.DOWN;
    // left
    if(tileDelta.Equals(directions[2])) return Dir.LEFT;
    // up
    if(tileDelta.Equals(directions[3])) return Dir.UP;

    throw new System.Exception("Maze.GetDirection - direction = none");
  }



// ==============================================================================
// =============== tile validity utility methods =============================-
// ==============================================================================
  public void WrapTile(ref Vector2Int tile)
  {
    // wrap x
    if(tile.x >= width) {
      tile.x -= width;
    } else if(tile.x < 0) {
      tile.x += width;
    }
    // wrap y
    if(tile.y >= height) {
      tile.y -= height;
    } else if(tile.y < 0) {
      tile.y += height;
    }
  }

  /*
   * TODO
   * use string as map instead of maze tile tileTypes
   *
   */


  public bool TileContainsPellet(Vector2Int tile){
      return pelletsTileTypes.GetTileID(tile) == MazeTileTypes.TileID.PELLET;
  }

  public bool TileContainsEnergizer(Vector2Int tile){
    return pelletsTileTypes.GetTileID(tile) == MazeTileTypes.TileID.ENERGIZER;
  }

  public bool TileIsPath(Vector2Int tile)
  {
    return pathsTileTypes.GetTileID(tile) == MazeTileTypes.TileID.PATH;
  }

  public bool TileIsGhostHouse(Vector2Int tile)
  {
    return ghosthouseTileTypes.GetTileID(tile)
      == MazeTileTypes.TileID.GHOST_HOUSE_WALL_TILE;
  }


  public bool TileGhostNoUpward(Vector2Int tile)
  {
    return ghostZonesTileTypes.GetTileID(tile) == MazeTileTypes.TileID.NO_UPWARD;
  }

  public bool TileIsGhostDoor(Vector2Int tile)
  {
    return ghostZonesTileTypes.GetTileID(tile) == MazeTileTypes.TileID.GHOST_DOOR;
  }

  public bool TileIsTeleport(Vector2Int tile)
  {
    if(TileIsPath(tile)) {
      if(tile.x == 0) {
        return TileIsPath(new Vector2Int(width - 1, tile.y));
      } else if(tile.x == (width - 1)) {
        return TileIsPath(new Vector2Int(0, tile.y));
      }
    }
    return false;
  }

  public bool TileIsTunnel(Vector2Int tile)
  {
    return ghostZonesTileTypes.GetTileID(tile) == MazeTileTypes.TileID.TUNNEL;
  }


// ==============================================================================
// =============== Tile map method ==========================================-
// ==============================================================================

  private void DrawTiles(MazeTileTypes tileTypes)
  {
    for(int j = 0; j < height; j++) {
      for(int i = 0; i < width; i++) {
        Vector2Int tile = new Vector2Int(i, j);
        MazeTileTypes.TileID tileID = tileTypes.GetTileID(tile);
        switch(tileID) {
          case MazeTileTypes.TileID.WALL:
            tilemapWalls.SetTile(new Vector3Int(tile.x, tile.y, 0), ruleTileRegWall);
            break;
          case MazeTileTypes.TileID.GHOST_HOUSE_WALL_TILE:
            tilemapWalls.SetTile(new Vector3Int(tile.x, tile.y, 0), ruleTileGhostHouse);
            break;
          case MazeTileTypes.TileID.GHOST_DOOR:
            // TODO - position the door based on a seperate image,
            //        so you can take the orientation into account
            tilemapGhostDoor.SetTile(new Vector3Int(tile.x, tile.y, 0), ruleTileGhostDoor);
            break;
          default:
            // default - do nothing
            break;
        }
      }
    }
  }
}
}
