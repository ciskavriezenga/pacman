using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostMovement : MonoBehaviour
{
  // current position of pacman, also used as start position
  public Vector2 currentPos = new Vector2(13.875f, 7.625f);
  // speed of pacman
  public float speed;
  // reference to the grid object
  public Grid grid;

  // temp reference to Pacman - to test Blinky pathfinding
  public PacmanMovement pacmanMov;

  // tile in the pacman maze grid
  private Vector2Int currentTile;

  // current movement direction
  private Grid.Dir currentDir = Grid.Dir.None;

  // move to this position
  private Vector2 moveToPos;


  // the oposite directions to a given direction
  private Grid.Dir[] oppositeDirs = {
    Grid.Dir.Up, // Up --> down
    Grid.Dir.Right, // Right --> Left
    Grid.Dir.Down, // Down --> up
    Grid.Dir.Left, // left --> right
  };

  // 2d array to quickly retrieve the allowed directions based on current
  // directions
  private Vector2Int[][] allowedDirs = {
    new Vector2Int[] {new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(1,0)},
    new Vector2Int[] {new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(0,-1)},
    new Vector2Int[] {new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(-1,0)},
    new Vector2Int[] {new Vector2Int(0,-1), new Vector2Int(-1,0), new Vector2Int(0,1)}
  };

  // next tiles to move to
  Queue<Vector2Int> moveToTile = new Queue<Vector2Int>();
  // next pixel position to move to
  Queue<Vector2Int> moveToPixel = new Queue<Vector2Int>();

  // Start is called before the first frame update
  void Start()
  {
    // fetch direct reference to grid object
    grid = grid.GetComponent<Grid>();
    // fetch direct reference to PacmanMovement object
    pacmanMov = pacmanMov.GetComponent<PacmanMovement>();
    // set the current tile based on current position
    currentTile = grid.GetTileCoordinate(currentPos);
    currentDir = Grid.Dir.Right;
    // fetch first adjacent tile in direction - for now assuming this is valid
    // TODO - fix this setup step
    Vector2Int moveTo = grid.GetAdjacentTile(currentTile, currentDir);
    moveToTile.Enqueue(moveTo);
    AddPixelCoordMoves(moveTo);
    AddTileMoves(moveToTile.Peek());
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    // move towards target position
    currentPos = Vector2.MoveTowards(currentPos, moveToPos, speed);

    // cache the coordinate of pixel corresponding to the current position
    Vector2Int currentPixelCoord = grid.PixelCoordinate(currentPos);
    // transform the pixel coordinate back to the floating point position
    transform.position = grid.Position(currentPixelCoord);

    if(currentPixelCoord.Equals(moveToPixel.Peek())) {
      // remove the reached pixel from the queue
      moveToPixel.Dequeue();
      // transform the next pixel coordinate to a position and
      // store it as the new position to move to
      moveToPos = grid.Position(moveToPixel.Peek());

      // check if we changed to another tile
      // if so --> update current tile and set new target position
      Vector2Int newTile = grid.GetTileCoordinate(transform.position);
      Debug.Log("Ghost - current tileCoord: " + currentTile.x + " " + currentTile.y);
      if(!currentTile.Equals(newTile)) {
        currentTile = newTile;
        // remove the reached tile from the queue
        Vector2Int moveTo = moveToTile.Dequeue();
        AddPixelCoordMoves(moveTo);
        AddTileMoves(moveToTile.Peek());
      }
    }
  }

  void AddTileMoves(Vector2Int fromTile)
  {
    // add a new tile to moveToTile queue, always 1 move in the future
    moveToTile.Enqueue(GetFutureMove(fromTile, currentDir));
  }

  void AddPixelCoordMoves(Vector2Int moveTo)
  {
    // add new pixel positions:
    //  - one to reach the edge of the new tile
    //  - one to ensure the movement goes through the center of a tile, to
    //    prevend preturns
    Vector2 edgePosition = grid.GetMoveToPos(moveTo, currentDir);
    Vector2Int edgePixelCoord = grid.PixelCoordinate(edgePosition);
    moveToPixel.Enqueue(edgePixelCoord);
    Vector2 centerPosition = grid.GetCenterPos(moveTo);
    Vector2Int centerPixelCoordinate = grid.PixelCoordinate(centerPosition);
    moveToPixel.Enqueue(centerPixelCoordinate);
  }


  // returns the best future move
  Vector2Int GetFutureMove(Vector2Int departureTile, Grid.Dir dir) {
    // retrieve adjacentTiles
    Vector2Int[] adjacentTiles = allowedDirs[(int)dir];
    for(int i = 0; i < 3; i++) {
      // add departureTile to tile direction
      adjacentTiles[i] = adjacentTiles[i] + (departureTile);
    }
    Debug.Log("CHECK THIS - adjacentTiles: " + adjacentTiles);

    // find the best move, most near to target tile
    int indexBestMove = -1;
    int smallestDistance = int.MaxValue;
    Vector2Int targetTile = GetTargetTile();
    for(int i = 0; i < 3; i++) {
      if(grid.TileIsPath(adjacentTiles[i])) {
        int distance =
          grid.SquaredEuclideanDistance(targetTile, adjacentTiles[i]);
        if(distance < smallestDistance) {
          smallestDistance = distance;
          indexBestMove = i;
        }
      }
    } // end forloop

    // return the best move
    return adjacentTiles[indexBestMove];
  }


  Vector2Int GetTargetTile() {
    return pacmanMov.currentTile;
  }

}
