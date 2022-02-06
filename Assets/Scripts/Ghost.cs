using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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

  // move to this position
  private Vector2 moveToPos;

  // the oposite directions to a given direction
  private Grid.Dir[] oppositeDirs = {
    Grid.Dir.Down, // Up --> down
    Grid.Dir.Left, // Right --> Left
    Grid.Dir.Up, // Down --> up
    Grid.Dir.Right, // left --> right
  };

  // 2d array with the allowd directions based on current direction
  // prefered order up, left, down, right
  private Grid.Dir[][] allowedDirs = {
    new Grid.Dir[] {Grid.Dir.Up, Grid.Dir.Left, Grid.Dir.Right}, // cur = R
    new Grid.Dir[] {Grid.Dir.Up, Grid.Dir.Down, Grid.Dir.Right}, // cur = D
    new Grid.Dir[] {Grid.Dir.Left, Grid.Dir.Down, Grid.Dir.Right}, // cur = L
    new Grid.Dir[] {Grid.Dir.Up, Grid.Dir.Left, Grid.Dir.Down} // cur = U
  };




  // current tile and move
  public Vector2Int currentTile { get; private set; }
  Move currentMove;
  // next tiles to move to
  Queue<Move> nextMoves = new Queue<Move>();
  // allows to let the ghosts generate moves further ahead
  //   which adds a delay to their movement
  Move movesLastMove;

  // Start is called before the first frame update
  void Start()
  {
    // fetch direct reference to grid object
    grid = grid.GetComponent<Grid>();
    // fetch direct reference to PacmanMovement object
    pacmanMov = pacmanMov.GetComponent<PacmanMovement>();
    // set the current tile based on current position
    currentTile = grid.GetTileCoordinate(currentPos);

    // add first move - start up
    // TODO - fix this setup step
    currentMove = CreateSingleMove(currentTile, Grid.Dir.Right);
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
    if(currentMove.NextPixel()) {
      // new pixel move is available
      // do nothing, except when curren tile is teleport tile
      if(grid.TileIsTeleport(currentMove.tile)) {
        currentMove = nextMoves.Dequeue();
        // empty nextMoves
        nextMoves.Clear();

        // TODO - wrap below in a method and clean up init in start as well
        Vector2Int teleportPixel = currentMove.GetPixelMove();
        currentPos = grid.Position(teleportPixel);
        transform.position = grid.Position(teleportPixel);
        currentTile = currentMove.tile;
        currentMove = CreateSingleMove(currentTile, currentMove.direction);
        // generate the upcoming moves for the ghost - based on last generate move
        movesLastMove = currentMove;
        GenerateMoves();      
      }

    } else {
      // no new pixel move available - move is finalized
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

  Move CreateSingleMove(Vector2Int fromTile, Grid.Dir direction)
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
        // only allow movement up if allowed
        if(directions[i] != Grid.Dir.Up || !grid.GhostMoveUpForbidden(fromTile)) {
          int distance =
            grid.SquaredEuclideanDistance(targetTile, adjacentTiles[i]);
          if(distance < smallestDistance) {
            smallestDistance = distance;
            indexBestMove = i;
          }
        }
      }
    } // end forloop

    // create a new Move struct
    // NOTE: corresponding pixels are calculated in Move constructor
    return new Move(adjacentTiles[indexBestMove], directions[indexBestMove], ref grid);

  }

  Vector2Int[] GetAdjacentTiles(Vector2Int departureTile, Grid.Dir[] directions)
  {
    // copy relative adjacentTiles that correspond with the allowed directions
    Vector2Int[] adjacentTiles = {
      grid.directions[(int) directions[0]],
      grid.directions[(int) directions[1]],
      grid.directions[(int) directions[2]]
    };

    // add the departure tile position to relative adjactentTiles
    for(int i = 0; i < 3; i++) {
      adjacentTiles[i] = adjacentTiles[i] + (departureTile);
      grid.WrapTile(ref adjacentTiles[i]);
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
