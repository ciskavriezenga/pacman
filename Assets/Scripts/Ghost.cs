using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct GhostMove {
  public Vector2Int tile { get; private set; }
  public Grid.Dir direction;
  private Vector2Int[] pixels;
  private int pixelIndex;

  public GhostMove(Vector2Int tile, Grid.Dir direction, ref Grid grid) {
    this.tile = tile;
    this.direction = direction;
    pixelIndex = 0;
    // fetch the pixel locations for this move
    Vector2 edgePosition = grid.GetMoveToPos(tile, direction);
    Vector2 centerPosition = grid.GetCenterPos(tile);
    pixels = new Vector2Int[]{
      grid.PixelCoordinate(edgePosition),
      grid.PixelCoordinate(centerPosition)
    };
  }

  public Vector2Int GetPixelMove()
  {
    return pixels[pixelIndex];
  }

  public bool NextPixel() {
    Debug.Log("NextPixel - index: " + pixelIndex);
    // change pixel index to second pixel, if current index is 0
    if(pixelIndex == 0 ) {
      pixelIndex = 1;
      return true;
    }
    return false; // no next pixel
  }

  public void Log() {
    Debug.Log("Ghostmove-Tile: " + tile
      + ", direction: " + direction
      + ", pixelIndex: " + pixelIndex
      + ", pixels[0]: " + pixels[0]
      + ", pixels[1]: " + pixels[1]);
  }
}

public class Ghost : MonoBehaviour
{
  // current position of pacman, also used as start position
  public Vector2 currentPos = new Vector2(13.875f, 7.625f);
  // speed of pacman
  public float speed;
  // reference to the grid object
  public Grid grid;

  // temp reference to Pacman - to test Blinky pathfinding
  public PacmanMovement pacmanMov;

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

  private Grid.Dir[][] allowedDirs = {
    new Grid.Dir[] {Grid.Dir.Left, Grid.Dir.Up, Grid.Dir.Right},
    new Grid.Dir[] {Grid.Dir.Up, Grid.Dir.Right, Grid.Dir.Down},
    new Grid.Dir[] {Grid.Dir.Right, Grid.Dir.Down, Grid.Dir.Left},
    new Grid.Dir[] {Grid.Dir.Down, Grid.Dir.Left, Grid.Dir.Up}
  };

  // 2d array to quickly retrieve the allowed directions based on current
  // directions
  // private Vector2Int[][] allowedDirs = {
  //   new Vector2Int[] {new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(1,0)},
  //   new Vector2Int[] {new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(0,-1)},
  //   new Vector2Int[] {new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(-1,0)},
  //   new Vector2Int[] {new Vector2Int(0,-1), new Vector2Int(-1,0), new Vector2Int(0,1)}
  // };

  // current tile and move
  public Vector2Int currentTile { get; private set; }
  GhostMove currentMove;
  // next tiles to move to
  Queue<GhostMove> nextMoves = new Queue<GhostMove>();
  // allows to let the ghosts generate moves further ahead
  //   which adds a delay to their movement
  GhostMove movesLastMove;

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

    // add first move - start up
    // TODO - fix this setup step
    currentMove = CreateSingleMove(currentTile, currentDir);
    // generate the upcoming moves for the ghost - based on last generate move
    movesLastMove = currentMove;
    GenerateMoves();

    // get the target position based on first queued pixel move position
    moveToPos = grid.Position(currentMove.GetPixelMove());
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    // move towards target position
    currentPos = Vector2.MoveTowards(currentPos, moveToPos, speed);

    // get the coordinate of pixel corresponding to the current position
    Vector2Int currentPixelCoord = grid.PixelCoordinate(currentPos);
    // transform the pixel coordinate back to the floating point position
    transform.position = grid.Position(currentPixelCoord);

    // if the current pixel coordinate corresponds to the current pixel move
    if(currentPixelCoord.Equals(currentMove.GetPixelMove())) {
      UpdateMoves();
    }
  }

  void UpdateMoves()
  {
    // we are ready for the next pixel in the current move
    // if this does not exist, we need a new move
    if(!currentMove.NextPixel()) {
      // retrieve a new move from the moves queue
      currentMove = nextMoves.Dequeue();
    }
    // check if we reached the next tile
    currentTile = grid.GetTileCoordinate(transform.position);
    if(currentTile == currentMove.tile) {
      // add a new move based on last move in queue
      GenerateMoves();
    }
    // transform the next pixel coordinate to a position and
    // store it as the new position to move to
    moveToPos = grid.Position(currentMove.GetPixelMove());
  }

  void GenerateMoves()
  {
    // TODO - use a numMovesAhead variable
    //        to store more or less moves ahead in time
    movesLastMove = CreateSingleMove(movesLastMove.tile, movesLastMove.direction);
    nextMoves.Enqueue(movesLastMove);
  }

  GhostMove CreateSingleMove(Vector2Int fromTile, Grid.Dir direction)
  {
    Vector2Int targetTile = GetTargetTile();
    // get the tile of the new move
    // retrieve adjacentTiles
    Grid.Dir[] directions = allowedDirs[(int) direction];
    Vector2Int[] adjacentTiles = GetAdjacentTiles(fromTile, directions);
    // find the best move, most near to target tile
    int indexBestMove = -1;
    int smallestDistance = int.MaxValue;
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

    // create a new GhostMove struct
    // NOTE: corresponding pixels are calculated in GhostMove constructor
    return new GhostMove(adjacentTiles[indexBestMove], directions[indexBestMove], ref grid);

  }

  Vector2Int[] GetAdjacentTiles(Vector2Int departureTile, Grid.Dir[] directions)
  {
    // copy relative adjacentTiles that correspond with the allowed directions
    Vector2Int[] adjacentTiles = {
      grid.directions[(int) directions[0]],
      grid.directions[(int) directions[1]],
      grid.directions[(int) directions[2]]
    };
    // TODO - remove outcommented code when done with refactor
    // System.Array.Copy(allowedDirs[(int)dir], adjacentTiles, 3) ;
    // add the departure tile position to relative adjactentTiles
    for(int i = 0; i < 3; i++) {
      adjacentTiles[i] = adjacentTiles[i] + (departureTile);
    }
    // return resulting adjacent tiles
    return adjacentTiles;
  }

  Vector2Int GetTargetTile()
  {
    // TODO - override in subclases
    return pacmanMov.currentTile;
  }

}
