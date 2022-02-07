using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Ghost : MonoBehaviour
{
  public enum ChaseMode
  {
    TargetPacman = 0, // regular Blinky behaviour
    Frightened = 1,
    Chase = 2
  }
  /*
   * in range x [0, 27], width = 28 and y [0, 30], height = 31
   * scatter tile Blinky: x = 25, y = 32
   * scatter tile Pinky: ..
   * scatter tile ...: ...
   * scatter tile ...: ...
   */
  public Transform scatterPos;
  private Vector2Int scatterTile;
  // current position of pacman, also used as start position
  public Vector2 currentPos = new Vector2(13.875f, 7.625f);
  // currentSpeed of pacman
  private float currentSpeed;
  public float slowDownSpeedMultiplier;
  public float normalSpeed;
  private bool isSlowedDown = false;

  // chase modes e.g. target pacman (Blinky)
  public ChaseMode chaseMode = ChaseMode.TargetPacman;
  // ghost mode, either scatter, frightened, chase
  public GameManager gameManager;
  public GhostMode currentGhostMode;


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


// =============================================================================
// =============== Initialize methods ==========================================
// =============================================================================

  // Start is called before the first frame update
  void Start()
  {
    // fetch direct reference to grid object
    grid = grid.GetComponent<Grid>();
    // fetch direct reference to PacmanMovement object
    pacmanMov = pacmanMov.GetComponent<PacmanMovement>();
    // fetch direct reference to GameManager object;
    gameManager = gameManager.GetComponent<GameManager>();

    // cache the current ghost mode
    currentGhostMode = gameManager.currentGhostMode;

    // transform scatter pos to scatter TileCoordscatterTile
    scatterTile = grid.GetTileCoordinate(scatterPos.position);
    // set the current tile based on current position
    currentTile = grid.GetTileCoordinate(currentPos);
    // set currentSpeed based on the normal speed value
    currentSpeed = normalSpeed;

    // add first move - start up
    // TODO - fix this setup step
    currentMove = CreateSingleMove(currentTile, Grid.Dir.Right);
    // generate the upcoming moves for the ghost - based on last generate move
    movesLastMove = currentMove;
    GenerateMoves();

    // get the target position based on first queued pixel move position
    moveToPos = grid.Position(currentMove.GetPixelMove());
  }

// =============================================================================
// =============== Update methods ==============================================
// =============================================================================

  // Update is called once per frame
  void FixedUpdate()
  {
    // move currentPos closer towards target position
    currentPos = Vector2.MoveTowards(currentPos, moveToPos, currentSpeed);

    // get the coordinate of the pixel corresponding to the current position
    Vector2Int currentPixelCoord = grid.PixelCoordinate(currentPos);

    // transform the pixel coordinate back to the floating point position
    // and update the position of the ghost
    transform.position = grid.Position(currentPixelCoord);

    // if the current pixel coordinate corresponds to the current pixel
    // coordinate in our current move --> we reached a pixel target
    // thus, we can update our current move or retrieve a new one
    if(currentPixelCoord.Equals(currentMove.GetPixelMove())) {
      UpdateMove();
    }
  }

  void UpdateMove()
  {
    // we are ready for the next pixel target coordinate in the current move
    // if this does not exist, we need a new move
    if(currentMove.NextPixel()) {
      // there is a new pixel target coordination available
      // but did we reached a new tile?
      currentTile = grid.GetTileCoordinate(transform.position);
      if(currentTile == currentMove.tile) {
        // generate a new move
        GenerateMoves();
        // reached a new tile --> process its type (teleport, slower speed, ..)
        ProcessCurrentTileType();
      }
    } else {
      // no new pixel move available - move is finalized
      // retrieve a new move from the moves queue
      currentMove = nextMoves.Dequeue();
    }

    // transform the next pixel coordinate to a position and
    // store it as the new position to move to
    moveToPos = grid.Position(currentMove.GetPixelMove());
  }

// =============================================================================
// =============== Generate moves methods ======================================
// =============================================================================

  void GenerateMoves()
  {
    // TODO - use a numMovesAhead variable
    //        to store more or less moves ahead in time

    // ghosts generate moves ahead in time
    // therefore, add a new move based on last move in our queue
    movesLastMove = CreateSingleMove(movesLastMove.tile, movesLastMove.direction);
    nextMoves.Enqueue(movesLastMove);
  }

  Move CreateSingleMove(Vector2Int fromTile, Grid.Dir direction)
  {
    // TODO - add check if frightened - up tile movement is allowed!
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
        if(directions[i] != Grid.Dir.Up || !grid.TileGhostNoUpward(fromTile)) {
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

// =============================================================================
// =============== Proces tile type methods ====================================
// =============================================================================

  // processes the current tile type if necessary
  void ProcessCurrentTileType() {
    MazeTileTypes.TileID tileID = grid.GetTileType(currentTile);

    switch(tileID) {
      case MazeTileTypes.TileID.Teleport: {
        Teleport();
        break;
      } // end case teleport
      case MazeTileTypes.TileID.Tunnel: {
        SlowDownSpeed();
        break;
      } // end case tunnel
      default: {
        ResetSpeed();
        break;
      } // end default case
    } // end switch tileID
  }

  // teleports the gost to the otherside
  void Teleport(){
    currentMove = nextMoves.Dequeue();
    // reset moves
    ResetMoves(currentMove);
  }
  void ResetMoves(Move currentMove) {
    // empty nextMoves
    nextMoves.Clear();
    Vector2Int teleportPixel = currentMove.GetPixelMove();
    currentPos = grid.Position(teleportPixel);
    transform.position = grid.Position(teleportPixel);
    currentTile = currentMove.tile;
    currentMove = CreateSingleMove(currentTile, currentMove.direction);
    // generate the upcoming moves for the ghost - based on last generate move
    movesLastMove = currentMove;
    GenerateMoves();
  }

  // slows down current speed based on normal speed an slowDownSpeedMultiplier
  void SlowDownSpeed()
  {
    if(!isSlowedDown) {
      isSlowedDown = true;
      currentSpeed = normalSpeed * slowDownSpeedMultiplier;
    }
  }

  // resets current speed based on normal speed value
  void ResetSpeed() {
    if(isSlowedDown) {
      // reset speed
      isSlowedDown = false;
      currentSpeed = normalSpeed;
    }
  }

// =============================================================================
// =============== Other methods ===============================================
// =============================================================================

  // returns the target tile -
  // TODO - method returns path based on ghosttype
  Vector2Int GetTargetTile()
  {
    // return the tile that corresponds to the current mode
    // NOTE: Ghosts use a pseudo-random number generator (PRNG) to pick a way
    // to turn at each intersection when frightened. - no target tile needed
    // therefore only two relavnt options: either scatter or Chase
    if(currentGhostMode == GhostMode.Chase) {
      // TODO - use chaseMode in switch
      return pacmanMov.currentTile;
    }
    return scatterTile;
  }

  public void SwitchMode(GhostMode newGhostMode) {
    Debug.Log("*** Ghost.SwitchMode - new mode: " + newGhostMode + " ***");

    /*
     * Ghosts are forced to reverse direction by the system anytime the
     * mode changes from:
     *  • chase-to-scatter
     *  • chase-tofrightened
     *  • scatter-to-chase
     *  • scatter-to-frightened.
     * Ghosts do not reverse direction when:
     *  • frightened - chase
     *  • frightened - scatter
     * Reference: The Pacman Dosier - Gamasutra
     */
    if(currentGhostMode != GhostMode.Frightened) {
      SwitchDirection(ref currentMove);
    }
    // cache the new ghost mode
    currentGhostMode = newGhostMode;

    ResetMoves(currentMove);
  }

  void SwitchDirection(ref Move move) {
    move.direction = oppositeDirs[(int) move.direction];
  }


}
