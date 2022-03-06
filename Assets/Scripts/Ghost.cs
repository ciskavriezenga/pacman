//#define SHOW_GHOST_TARGET_TILE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PM {

public class Ghost : MonoBehaviour
{
  // --------------- directions and related -----------------------------------
  // enum to for the different chase schemes
  public enum ChaseScheme
  {
    TARGET_PACMAN = 0,   // regular Blinky behaviour
    AHEAD_OF_PACMAN = 1,  // regular Pinky behaviour
    COLLABORATE = 2,    // regular Inky behaviour, based on blinky pos and pm
    CIRCLE_AROUND = 3,   // regular Clyde behaviour
  }

  // the oposite directions to a given direction
  private Dir[] oppositeDirs = {
    Dir.LEFT, // Right --> Left
    Dir.UP, // Down --> up
    Dir.RIGHT, // left --> right
    Dir.DOWN // Up --> down
  };

  // 2d array with the allowed directions based on current direction
  // prefered order up, left, down, right
  private Dir[][] allowedDirs = {
    new Dir[] {Dir.UP, Dir.DOWN, Dir.RIGHT}, // cur = R
    new Dir[] {Dir.LEFT, Dir.DOWN, Dir.RIGHT}, // cur = D
    new Dir[] {Dir.UP, Dir.LEFT, Dir.DOWN}, // cur = L
    new Dir[] {Dir.UP, Dir.LEFT, Dir.RIGHT} // cur = U
  };


  // --------------- references -----------------------------------------------
  // reference to other objects - necessary to react
  [SerializeField] private GameManager gameManager;
  [SerializeField] private Maze maze;
  [SerializeField] private Pacman pacman;

  // the settings - used for initiate and reset
  private GhostSettings settings;

  // current position of ghost, also used as start position
  [SerializeField] private Vector2 currentPos;

  // fields related to speed of ghost
  //[SerializeField] private float currentSpeed;
  [SerializeField] private float currentSpeed;

  // ghost mode, either scatter, frightened, chase
  private GhostMode currentGhostMode;
  // the ghost that will be used in case of the collaborate scheme
  // which corresponds to the original Inky behavior based on Blinky's pos
  private Ghost wingman;

  // move to this position
  private Vector2 moveToPos;

  // current tile
  [SerializeField] private Vector2Int currentTile;
  // moves - hold the moves one step ahead in time, current and last moves
  private Queue<Move> nextMoves = new Queue<Move>();
  private Move currentMove;
  private Dir currentDir;
  // TODO - this movelastmove  - better + easier + clearer flow possible?
  private Move movesLastMove;
  // TODO test idea: allows to let the ghosts generate moves further ahead
  //   which adds a delay to their movement

  #if SHOW_GHOST_TARGET_TILE
    //public Vector2Int scatterPos;
    private GameObject targetTileSR;
    private GameObject scatterTileSR;
  #endif

  [SerializeField] private Animator animator;
  [SerializeField] private RuntimeAnimatorController regularAnimatorController;
  [SerializeField] private RuntimeAnimatorController scaredAnimatorController;

  // ghost house fields
  bool waitingInGhostHouse;


// =============================================================================
// =============== Initialize methods ==========================================
// =============================================================================

  public void Initialize (GhostSettings settings, GameManager gameManager) {
    // store settings struct and gameManager, maze and pacman references
    this.settings = settings;
    this.gameManager = gameManager;
    this.maze = gameManager.GetMaze();
    this.pacman = gameManager.GetPacman();

    regularAnimatorController = Resources.Load(settings.name)
      as RuntimeAnimatorController;
    scaredAnimatorController =  Resources.Load("ghost_scared")
      as RuntimeAnimatorController;

    animator = GetComponent<Animator>();
    animator.runtimeAnimatorController = regularAnimatorController;
    // cache the current ghost mode
    if(settings.startInGhosthouse) {
      currentGhostMode = GhostMode.WAITING_IN_HOUSE;
    } else {
      gameManager.currentGhostMode;
    }

    // set initial values
    currentTile = maze.GetTileCoordinate(settings.startPos);
    currentPos = settings.startPos;
    //waitingInGhostHouse = settings.startInGhosthouse;

    currentSpeed = settings.normSpeed;

    // add first move - start up
    // TODO - fix this setup step - pos in ghost house
    currentMove = CreateSingleMove(currentTile, settings.startDirection);

    // generate the upcoming moves for the ghost - based on last generate move
    movesLastMove = currentMove;
    GenerateMoves();

    // get the target position based on first queued pixel move position
    moveToPos = maze.Position(currentMove.GetPixelMove());
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
    Vector2Int currentPixelCoord = maze.PixelCoordinate(currentPos);

    // transform the pixel coordinate back to the floating point position
    // and update the position of the ghost
    transform.position = maze.Position(currentPixelCoord);

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
      currentTile = maze.GetTileCoordinate(transform.position);
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
      SetDir(currentMove.direction);
    }

    // transform the next pixel coordinate to a position and
    // store it as the new position to move to
    moveToPos = maze.Position(currentMove.GetPixelMove());
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

  Move CreateSingleMove(Vector2Int fromTile, Dir direction)
  {
    // ---------------- FRIGHTENED ------------------------------------
    switch(currentGhostMode) {
      case GhostMode.PACING_HOME:
        break;
      case GhostMode.LEAVING_HOME:
        break;
      case GhostMode.FRIGHTENED:
        return CreateFrightenedMove(fromTile, direction);
    }
    // ---------------- CHASE OR SCATTER ------------------------------
    return CreateChaseScatterMove(fromTile, direction);
  }

  private Move CreateFrightenedMove(Vector2Int fromTile, Dir direction)
  {
    // "attempts the remaining directions in this order:
    // up, left, down, and right"
    // NOTE: up movement on no-upmovement tiles is allowed while frightened
    Dir dir = GetRandomDirection();
    Vector2Int randomTile = maze.GetAdjacentTile(fromTile, dir);

    int numTimes = 0;
    while(dir == oppositeDirs[(int) direction] || !maze.TileIsPath(randomTile)) {
      Debug.Log("FRIGHTENED - not validate random move in direction "
        + dir
        + ", current direction: " + direction
        + ", from tile: " + fromTile
        + ", random tile = " + randomTile
        + ", tile is path: " + maze.TileIsPath(randomTile)
      );
      dir = (Dir) ((int)dir - 1);
      // wrap direction if equal NONE  (-1)
      if(dir == Dir.NONE) dir = Dir.UP;
      randomTile = maze.GetAdjacentTile(fromTile, dir);
      if(numTimes > 4) {
        Debug.Log("FRIGHTENED - ******* ERROR ******* should not happen");
        break;
      }
      numTimes++;
    }
    Debug.Log("FRIGHTENED - found a valid move in direction "
      + ", current direction: " + direction
      + ", from tile: " + fromTile
      + ", random tile = " + randomTile
      + ", tile is path: " + maze.TileIsPath(randomTile)
    );
    // create a new Move struct
    // NOTE: corresponding pixels are calculated in Move constructor
    return new Move(randomTile, dir, maze);
  }

  private Move CreateChaseScatterMove(Vector2Int fromTile, Dir direction)
  {
    // retrieve adjacentTiles
    Dir[] directions = allowedDirs[(int) direction];
    Vector2Int[] adjacentTiles = GetAdjacentTiles(fromTile, directions);


    // return the tile that corresponds to the current mode
    // NOTE: Ghosts use a pseudo-random number generator (PRNG) to pick a way
    // to turn at each intersection when frightened. - no target tile needed
    // therefore only two relavnt options: either scatter or CHASE
    Vector2Int targetTile;
    if(currentGhostMode == GhostMode.CHASE) {
       = settings.scatterTile;
    } else {
      targetTile = settings.scatterTile;
    }

#if SHOW_GHOST_TARGET_TILE
    targetTileSR.transform.position = maze.GetCenterPos(targetTile);
#endif
    // find the best move, most near to target tile
    int indexBestMove = -1;
    int smallestDistance = int.MaxValue;
    for(int i = 0; i < 3; i++) {
      if(maze.TileIsPath(adjacentTiles[i])
        || maze.TileIsGhostDoor(adjacentTiles[i]) && directions[i] == Dir.UP
        && gameManager.PathIsEmpty(adjacentTiles[i])) {
        // only allow movement up if allowed
        if(directions[i] != Dir.UP || !maze.TileGhostNoUpward(fromTile)) {
          int distance =
            maze.SquaredEuclideanDistance(targetTile, adjacentTiles[i]);
          if(distance < smallestDistance) {
            smallestDistance = distance;
            indexBestMove = i;
          }
        }
      }
    } // end forloop
    Debug.Log("CreateChaseScatterMove- indexBestMove: " + indexBestMove);
    // create a new Move struct
    // NOTE: corresponding pixels are calculated in Move constructor
    if(indexBestMove == -1) {
      if(maze.TileIsGhostHouse(fromTile)) {
          // swap direction
          SwitchDirection(ref direction);
          return CreateChaseScatterMove(fromTile, direction);
      } else {
        Debug.Log("***** SHOULD NOT HAPPEN");
      }
    }
    return new Move(adjacentTiles[indexBestMove], directions[indexBestMove], maze);
  }


  private Dir GetRandomDirection() {
    return (Dir) (Random.value * 3.999999999999f);
  }

  Vector2Int[] GetAdjacentTiles(Vector2Int departureTile, Dir[] directions)
  {
    // copy relative adjacentTiles that correspond with the allowed directions
    Vector2Int[] adjacentTiles = {
      maze.directions[(int) directions[0]],
      maze.directions[(int) directions[1]],
      maze.directions[(int) directions[2]]
    };

    // add the departure tile position to relative adjactentTiles
    for(int i = 0; i < 3; i++) {
      adjacentTiles[i] = adjacentTiles[i] + departureTile;
      maze.WrapTile(ref adjacentTiles[i]);
    }
    // return resulting adjacent tiles
    return adjacentTiles;
  }

// =============================================================================
// =============== Proces tile type methods ====================================
// =============================================================================

  // processes the current tile type if necessary
  void ProcessCurrentTileType() {
    if(maze.TileIsTeleport(currentTile)){
      Teleport();
    } else if (currentGhostMode == GhostMode.FRIGHTENED) {
      currentSpeed = settings.frightSpeed;
    } else {
      if (maze.TileIsTunnel(currentTile)) {
        currentSpeed = settings.tunnelSpeed;
      } else {
        currentSpeed = settings.normSpeed;
      }
    }
  }

  // teleports the gost to the otherside
  void Teleport(){
    currentMove = nextMoves.Dequeue();
    // reset moves
    ResetMoves(currentMove);
  }


  void ResetMoves(Move move) {
    // empty nextMoves
    nextMoves.Clear();

    // set currentposition to pixel position in the passed move struct
    Vector2Int pixelMove = move.GetPixelMove();
    currentPos = maze.Position(pixelMove);
    transform.position = maze.Position(pixelMove);

    // set current tile to the tile in the passed move struct
    currentTile = move.tile;
    // create the first move and store to currentMove
    move = CreateSingleMove(currentTile, move.direction);

    // generate the upcoming moves for the ghost - based on last generate move
    movesLastMove = move;
    GenerateMoves();
  }


// =============================================================================
// =============== Target tile methods =========================================
// =============================================================================


  Vector2Int GetChaseTargetTile()
  {
    Vector2Int targetTile = new Vector2Int(0,0);

    /*
     * NOTE:  all information below about the schemes comes from the source:
     *  https://www.gamasutra.com/view/feature/3938/the_pacman_dossier.php?print=1
     */

    switch(settings.chaseScheme) {
      case ChaseScheme.TARGET_PACMAN:
        /*
         * NOTE:  Blinky's targeting scheme
         *        "Blinky's is the most simple and direct, using Pac-Man's
         *        current tile as his target."
         */

        return pacman.GetCurrentTile();

      case ChaseScheme.AHEAD_OF_PACMAN:
        /*
         * NOTE:  Pinky's targeting scheme
         *        "Pinky selects an offset four tiles away from Pac-Man in
         *        the direction Pac-Man is currently moving ...
         *        if Pac-Man is moving up, Pinky's target tile will be four
         *        tiles up and four tiles to the left. ... due to a subtle
         *        error in the logic code that calculates Pinky's offset from
         *        Pac-Man"
         */

        // call GetTileInDirection with addBugOffset set to true to include
        // the bug that results in an additional 4 tiles to the left if
        // Pac-Man's direction == Dir.UP

        return maze.GetTileInDirection(pacman.GetCurrentTile(),
          pacman.currentDir, 4, true);

      case ChaseScheme.COLLABORATE:
        /*
         * NOTE:  Inky's targeting scheme
         *        1. Draw line from Bliny to 2 tiles in front of Pac-man
         *        2. Double this line --> resulting end of line = target tile
         *        "Inky's offset calculation from Pac-Man is two tiles up and
         *         two tiles left when Pac-Man is moving up."
         */
        // get the tile 2 tiles in front of pacman
        Vector2Int tilesInFrontOfPM = maze.GetTileInDirection(pacman.GetCurrentTile(),
           pacman.currentDir, 2, true);

        // get wingman position (Blinky in normal configuration)
        Vector2Int targetVector = tilesInFrontOfPM - wingman.GetCurrentTile();

        // double targetVector
        targetVector = targetVector * 2;
#if DEBUG_GHOST
        Debug.Log("------------------------- (wingman.currentTile + targetTile + targetVector: " + (wingman.currentTile + targetVector));
#endif
        // add to the wingman's current tile
        return wingman.currentTile + targetVector;
      case ChaseScheme.CIRCLE_AROUND:
        /*
         * Note:  Clyde's targeting scheme
         *        "When more than eight tiles away, he uses Pac-Man's tile as
         *         his target ...
         *         If Clyde is closer than eight tiles away, he switches to
         *        his scatter mode target instead ... until he is far enough
         *         away to target Pac-Man again.
         */
         float distanceToPM  = (currentTile - pacman.GetCurrentTile()).magnitude;
         if(distanceToPM > 8) {
           return pacman.GetCurrentTile();
         }
         return settings.scatterTile;

      default:
        throw new System.Exception("Ghost.GetGhostTypeTargetTile - " +
          "ChaseScheme not found.");
    } // end switch chaseScheme
  }

  public void SetWingman(Ghost wingman)
  {
    this.wingman = wingman;
  }
/*
 * ---------- DEBUGGING - game object + sprite renderer  -------------------
 *            for display of target and scatter tiles
 */
#if SHOW_GHOST_TARGET_TILE
  public void SetDebugTargetTileSRs(GameObject targetTileSR, GameObject scatterTileSR)
  {
    this.targetTileSR = targetTileSR;
    this.scatterTileSR = scatterTileSR;
  }
#endif
// =============================================================================
// =============== Other methods ===============================================
// =============================================================================
  public void SwitchMode(GhostMode newGhostMode) {
    // do nothing if new mode equals current
    // NOTE: this should never happen - extra safe check though to be sure
    if(newGhostMode == currentGhostMode) {
      Debug.Log("********* ERROR *********\n Ghost.SwitchMode - new ghost mode is same as current!");
      return;
    }

#if DEBUG_GHOST
    Debug.Log("*** Ghost.SwitchMode - new mode: " + newGhostMode + " ***");
#endif
    /*
     *
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

    // Do NOT change mode if the ghost is waiting in the ghosthouse
    if(currentGhostMode != WAITING_IN_HOUSE) {
      if(currentGhostMode == GhostMode.FRIGHTENED) {
        // current mode is frightened, new mode not --> animation back to normal
        animator.runtimeAnimatorController = regularAnimatorController;
      } else {
        if(newGhostMode == GhostMode.FRIGHTENED) {
          // switch to scared animation controller
          animator.runtimeAnimatorController = scaredAnimatorController;
        }
          SwitchDirection(ref currentMove);
      }
      // cache the new ghost mode
      currentGhostMode = newGhostMode;

      ResetMoves(currentMove);
    }
  }

  void SwitchDirection(ref Move move)
  {
    move.direction = oppositeDirs[(int) move.direction];
  }

  void SwitchDirection(ref Dir dir)
  {
    dir = oppositeDirs[(int) dir];
  }

  void SetDir(Dir dir)
  {
    if(dir != currentDir) {
      animator.SetInteger("direction", (int) dir);
      currentDir = dir;
    }
  }
  public Vector2Int GetCurrentTile() { return currentTile;}
}
}
