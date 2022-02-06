using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// A model for the tile types in the pacman maze,
// where the types are based on a given background image
public class MazeTileTypes
{
  public enum TileID
  {
    RegWall = 0,    // black -  regular wall
    RegPath = 1,    // white -  regular path
    GhostHouse = 2, // red -    the ghost house - necessary for styling
    Tunnel = 3,     // green -  the tunnel, path where the ghosts move slower
    GhostDoor = 4,  // blue -   the exit of the ghost house
    NoUpward = 5,   // yellow - path where upward turns are forbidden for the ghosts
    EmptySlot1 = 6, // cyan -   ...
    EmptySlot2 = 7  // magenta -...
  }

  // tile IDs, one for each tile in the maze image
  public TileID[] tileIDs { get; private set; }

  // the with and the heigth of the maze
  private int width = 0;
  private int height = 0;


  // color lookup table, color position results in tile ID for the given color
  private static Color[] colorLookup = {
    Color.black, Color.white,
    Color.red, Color.green, Color.blue,
    new Color(1, 1, 0), Color.cyan, Color.magenta
  };

  // constructor - initializes the tileIDs
  public MazeTileTypes(string imgPath, int width, int height) {
    this.width = width;
    this.height = width;
    // TODO - add safety check if file exists
    // retrieve image data
    byte[] imgData = System.IO.File.ReadAllBytes(imgPath);
    Texture2D gridTexture2D = new Texture2D(width, height);
    gridTexture2D.LoadImage(imgData);

    // get array with the colors in image texture2d
    Color[] gridPixels = gridTexture2D.GetPixels();

    tileIDs = new TileID[width * height];
    // transform the colors to tile IDs
    for (int i = 0; i < gridPixels.Length; i++) {
      tileIDs[i] = TileIDForColor(gridPixels[i]);
    }

  }

  // returns the tileID at the given tile coordinate
  public TileID GetTileID(Vector2Int tileCoord) {
    int gridPixelsIndex = tileCoord.x + (tileCoord.y * width);
    return tileIDs[gridPixelsIndex];
  }

  // returns true if the tile coordinate corresponds to a walkable path
  public bool TileIsPath(Vector2Int tileCoord) {
    // bitshift the tile id 1 spot to the right
    TileID id = GetTileID(tileCoord);
    return ((byte)GetTileID(tileCoord) & 1) == 1;
  }



  private TileID TileIDForColor(Color color) {
    /*
     * 0  black =    0,    0,    0        - not walkable
     * 1  white =    255,  255,  255      - walkable
     * 2  red =      255,  0,    0        - not walkable
     * 3  green =    0,    255,  0        - walkable
     * 4  blue =     0,    0,    255      - not walkable
     * 5  yellow =   255,  255,  0        - walkable
     * 6  cyan =  0,    255,  255         - not walkable
     * 7  magenta =     255,  0,    255   - walkable
     */
    // NOTE: should not exceed 8, otherwise not in range of bytes
    for(byte i = 0; i < colorLookup.Length; i++) {
      if(color == colorLookup[i]){
        return (TileID)i;
      }
    }
    return TileID.RegWall; // default ID
  }

}
