using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM {
  public struct Move {
    public Vector2Int tile { get; private set; }
    public Maze.Dir direction;
    private Vector2Int[] pixels;
    private int pixelIndex;

    public enum MoveTypes {
      GhostRegular = 0,
      GhostTeleport = 1,
      PacmanRegular = 2,
      PacmanTeleport = 3
    };

    // TODO - remove ref
    public Move(Vector2Int tile, Maze.Dir direction, ref Maze maze) {
      this.tile = tile;
      this.direction = direction;
      pixelIndex = 0;
      // fetch the pixel locations for this move
      Vector2 edgePosition = maze.GetMoveToPos(tile, direction);
      Vector2 centerPosition = maze.GetCenterPos(tile);
      pixels = new Vector2Int[]{
        maze.PixelCoordinate(edgePosition),
        maze.PixelCoordinate(centerPosition)
      };
    }

    public Vector2Int GetPixelMove()
    {
      return pixels[pixelIndex];
    }

    public bool NextPixel() {
      // change pixel index to second pixel, if current index is 0
      if(pixelIndex == 0 ) {
        pixelIndex = 1;
        return true;
      }
      return false; // no next pixel
    }

    public Vector2Int GetCenterPixel()
    {
      // index 1 contains the center pixel coordinate
      return pixels[1];
    }

    public void Log() {
      Debug.Log("Ghostmove-Tile: " + tile
        + ", direction: " + direction
        + ", pixelIndex: " + pixelIndex
        + ", pixels[0]: " + pixels[0]
        + ", pixels[1]: " + pixels[1]);
    }
  }
}
