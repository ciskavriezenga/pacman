using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PM {
  // A model for the tile types in the pacman maze,
  // where the types are based on a given background image
  public class MazeTileTypes
  {
/*
 * 0  black =    0,    0,    0
 * 1  white =    255,  255,  255
 * 2  red =      255,  0,    0
 * 3  green =    0,    255,  0
 * 4  blue =     0,    0,    255
 * 5  yellow =   255,  255,  0
 * 6  cyan =     0,    255,  255
 * 7  magenta =  255,  0,    255
 * 8  gray =     125,  125,  125
 */


    public enum TileID
    {
      NONE = -1,
      // ---------- path and walls ----------
      WALL = 0, // black
      PATH = 1,// white

      // ghost specific zones
      GHOST_DOOR = 2, // red
      TUNNEL = 3, // green
      NO_UPWARD = 4, // blue

      // ---------- pellets ----------
      PELLET = 5, // yellow
      ENERGIZER = 6, // cyan

      // ---------- temporary solution ----------
      // to place correct double wall tiles
      GHOST_HOUSE_WALL_TILE = 7 // magenta
    }

    // tile IDs, one for each tile in the maze image
    public TileID[] tileIDs { get; private set; }

    // the with and the heigth of the maze
    private int width = 0;
    private int height = 0;

    private Color[] tempColors;
    // color lookup table, color position results in tile ID for the given color
    private static Color[] colorLookup = {
      Color.black, Color.white,
      Color.red, Color.green, Color.blue,
      new Color(1f, 1f, 0f), Color.cyan, Color.magenta
    };

    // constructor - initializes the tileIDs
    public MazeTileTypes(string imgPath, int width, int height) {
      this.width = width;
      this.height = width;
      // TODO - add safety check if file exists
      // retrieve image data
      byte[] imgData = System.IO.File.ReadAllBytes(imgPath);
      // NOTE: width and height are not necessary
      // TODO - retrieve width and height from texture after loading image :)
      Texture2D gridTexture2D = new Texture2D(width, height);
      gridTexture2D.LoadImage(imgData);


      // get array with the colors in image texture2d
      Color[] gridPixels = gridTexture2D.GetPixels();
      // TODO - remove this when fixed issues
      tempColors = gridPixels;

      tileIDs = new TileID[width * height];
      // transform the colors to tile IDs
      for (int i = 0; i < gridPixels.Length; i++) {
        tileIDs[i] = TileIDForColor(gridPixels[i]);
      }

    }

    // returns the tileID at the given tile coordinate
    public TileID GetTileID(Vector2Int tile)
    {
      return tileIDs[tile.x + (tile.y * width)];
    }

    // returns the tileID at the given tile coordinate
    public TileID GetTileID(int index)
    {
      return tileIDs[index];
    }


    private TileID TileIDForColor(Color color)
    {
      for(int i = 0; i < colorLookup.Length; i++) {
        if(color == colorLookup[i]){
          return (TileID)i;
        }
      }
      return TileID.NONE; // default ID
    }


// =============================================================================
// =============== DEBUG methods ===============================================
// =============================================================================
    public void LogColors(int fromX, int fromY, int upToX, int upToY)
    {
      for(int j = fromY; j < upToY; j++) {
        for(int i = fromX; i < upToX; i++) {
            Debug.Log("Color at: " + i + ", " + j
            + " - " + tempColors[i + (j * width)]);
        }
      }
    }

    public void LogAllTiles()
    {
      LogTiles(0, 0, width, height);
    }

    public void LogTiles(int fromX, int fromY, int upToX, int upToY)
    {
      for(int j = fromY; j < upToY; j++) {
        for(int i = fromX; i < upToX; i++) {
          LogTile(new Vector2Int(i, j));
        }
      }
    }

    public void LogTile(Vector2Int tile)
    {
      Debug.Log("Tile: " + tile + " is of type " + GetTileID(tile));
    }

  }
}
