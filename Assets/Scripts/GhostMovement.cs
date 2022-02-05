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
    Grid.Dir.Down, // Up --> down
    Grid.Dir.Left, // Right --> Left
    Grid.Dir.Up, // Down --> up
    Grid.Dir.Right, // left --> right
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
    Debug.Log("Ghost - currentPos: " + currentPos.x + " " + currentPos.y);
    Debug.Log("Ghost - currentTile: " + currentTile.x + " " + currentTile.y);
    Debug.Log("Ghost - moveTo: " + moveTo.x + " " + moveTo.y);
    AddPixelCoordMoves(moveTo);
    AddTileMoves(moveTo);
    moveToPos = grid.Position(moveToPixel.Peek());
    Debug.Log("Ghost - moveToPos: " + moveToPos.x + " " + moveToPos.y);
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    // move towards target position
    currentPos = Vector2.MoveTowards(currentPos, moveToPos, speed);

    // cache the coordinate of pixel corresponding to the current position
    Vector2Int currentPixelCoord = grid.PixelCoordinate(currentPos);
    //Debug.Log("Ghost - currentPixelCoord: " + currentPixelCoord.x + " " + currentPixelCoord.y);
    //Debug.Log("Ghost - moveToPixel.Peek(): " + moveToPixel.Peek().x + " " + moveToPixel.Peek().y);
    // transform the pixel coordinate back to the floating point position
    transform.position = grid.Position(currentPixelCoord);

    if(currentPixelCoord.Equals(moveToPixel.Peek())) {
      // remove the reached pixel from the queue
      moveToPixel.Dequeue();
      // check if we changed to another tile
      // if so --> update current tile and set new target position
      Vector2Int newTile = grid.GetTileCoordinate(transform.position);
      if(!currentTile.Equals(newTile)) {
        currentTile = newTile;
        // TODO - use moveToTile instead of queue
        // TODO - use direction instead of queue
        // - -split up GetFutureMove to retrieve index of best move and thereby direction
        // remove the reached tile from the queue
        Vector2Int moveTo = moveToTile.Dequeue();
        currentDir = grid.GetDirectionAdjacentTiles(currentTile, moveTo);
        Debug.Log("Ghost - new move to tile - moveTo: " + moveTo.x + " " + moveTo.y);
        AddPixelCoordMoves(moveTo);
        AddTileMoves(moveTo);
      }
      // transform the next pixel coordinate to a position and
      // store it as the new position to move to
      moveToPos = grid.Position(moveToPixel.Peek());
      Debug.Log("Ghost - new position to move to: " + moveToPos.x + " " + moveToPos.y);
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
    Vector2Int[] adjacentTiles = new Vector2Int[3];
    System.Array.Copy(allowedDirs[(int)dir], adjacentTiles, 3) ;
    for(int i = 0; i < 3; i++) {
      // add departureTile to tile direction
      adjacentTiles[i] = adjacentTiles[i] + (departureTile);
      Debug.Log(" adjacentTiles[i]: " + adjacentTiles[i].x + " " + adjacentTiles[i].y);
    }
    Debug.Log("Ghost::GetFutureMove - departureTile: " + departureTile.x + " " + departureTile.y);
    // find the best move, most near to target tile
    int indexBestMove = -1;
    int smallestDistance = int.MaxValue;
    Vector2Int targetTile = GetTargetTile();
    Debug.Log("Ghost::GetFutureMove - targetTile: " + targetTile.x + " " + targetTile.y);
    for(int i = 0; i < 3; i++) {
      if(grid.TileIsPath(adjacentTiles[i])) {
        int distance =
          grid.SquaredEuclideanDistance(targetTile, adjacentTiles[i]);
          Debug.Log("Ghost::GetFutureMove - distance: " + distance);
        if(distance < smallestDistance) {
          smallestDistance = distance;
          indexBestMove = i;
        }
      }
    } // end forloop

    Debug.Log("best move index: " + indexBestMove + ", adjacentTiles[indexBestMove]:" + adjacentTiles[indexBestMove]);
    // return the best move
    return adjacentTiles[indexBestMove];
  }


  Vector2Int GetTargetTile() {
    return pacmanMov.currentTile;
  }

}
