//#define SHOW_GHOST_TARGET_TILE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PM {

public class Ghost : MonoBehaviour
{
  // --------------- directions and related -----------------------------------
  // enum to for the different chase schemes
  public enum CHASEScheme
  {
    TargetPacman = 0,   // regular Blinky behaviour
    AheadOfPacman = 1,  // regular Pinky behaviour
    Collaborate = 2,    // regular Inky behaviour, based on blinky pos and pm
    CircleAround = 3,   // regular Clyde behaviour
  }

  // the oposite directions to a given direction
  private Maze.Dir[] oppositeDirs = {
    Maze.Dir.Down, // Up --> down
    Maze.Dir.Left, // Right --> Left
    Maze.Dir.Up, // Down --> up
    Maze.Dir.Right, // left --> right
  };

  // 2d array with the allowed directions based on current direction
  // prefered order up, left, down, right
  private Maze.Dir[][] allowedDirs = {
    new Maze.Dir[] {Maze.Dir.Up, Maze.Dir.Left, Maze.Dir.Right}, // cur = R
    new Maze.Dir[] {Maze.Dir.Up, Maze.Dir.Down, Maze.Dir.Right}, // cur = D
    new Maze.Dir[] {Maze.Dir.Left, Maze.Dir.Down, Maze.Dir.Right}, // cur = L
    new Maze.Dir[] {Maze.Dir.Up, Maze.Dir.Left, Maze.Dir.Down} // cur = U
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
  public float currentSpeed;
  private float slowDownSpeedMultiplier = 0.5f;
  private bool isSlowedDown = false;

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
  private Maze.Dir currentDir;
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
    currentGhostMode = gameManager.currentGhostMode;

    // set initial values
    currentTile = settings.startTile;
    currentPos = maze.GetCenterPos(currentTile);

    currentSpeed = settings.normalSpeed;

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

  Move CreateSingleMove(Vector2Int fromTile, Maze.Dir direction)
  {
    // TODO - add check if frightened - up tile movement is allowed!
    Vector2Int targetTile = GetTargetTile();
#if SHOW_GHOST_TARGET_TILE
    targetTileSR.transform.position = maze.GetCenterPos(targetTile);
#endif
    // get the tile of the new move
    // retrieve adjacentTiles
    Maze.Dir[] directions = allowedDirs[(int) direction];
    Vector2Int[] adjacentTiles = GetAdjacentTiles(fromTile, directions);
    // find the best move, most near to target tile
    int indexBestMove = -1;
    int smallestDistance = int.MaxValue;
    for(int i = 0; i < 3; i++) {
      if(maze.TileIsPath(adjacentTiles[i])) {
        // only allow movement up if allowed
        if(directions[i] != Maze.Dir.Up || !maze.TileGhostNoUpward(fromTile)) {
          int distance =
            maze.SquaredEuclideanDistance(targetTile, adjacentTiles[i]);
          if(distance < smallestDistance) {
            smallestDistance = distance;
            indexBestMove = i;
          }
        }
      }
    } // end forloop

    // create a new Move struct
    // NOTE: corresponding pixels are calculated in Move constructor


    // TODO - remove ref
    return new Move(adjacentTiles[indexBestMove], directions[indexBestMove], ref maze);

  }

  Vector2Int[] GetAdjacentTiles(Vector2Int departureTile, Maze.Dir[] directions)
  {
    // copy relative adjacentTiles that correspond with the allowed directions
    Vector2Int[] adjacentTiles = {
      maze.directions[(int) directions[0]],
      maze.directions[(int) directions[1]],
      maze.directions[(int) directions[2]]
    };

    // add the departure tile position to relative adjactentTiles
    for(int i = 0; i < 3; i++) {
      adjacentTiles[i] = adjacentTiles[i] + (departureTile);
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
    MazeTileTypes.TileID tileID = maze.GetTileType(currentTile);

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

  // slows down current speed based on normal speed an slowDownSpeedMultiplier
  void SlowDownSpeed()
  {
    if(!isSlowedDown) {
      isSlowedDown = true;
      currentSpeed = settings.normalSpeed * slowDownSpeedMultiplier;
    }
  }

  // resets current speed based on normal speed value
  void ResetSpeed() {
    if(isSlowedDown) {
      // reset speed
      isSlowedDown = false;
      currentSpeed = settings.normalSpeed;
    }
  }

// =============================================================================
// =============== Target tile methods =========================================
// =============================================================================

  // returns the target tile -
  // TODO - method returns path based on ghosttype
  Vector2Int GetTargetTile()
  {
    // return the tile that corresponds to the current mode
    // NOTE: Ghosts use a pseudo-random number generator (PRNG) to pick a way
    // to turn at each intersection when frightened. - no target tile needed
    // therefore only two relavnt options: either scatter or CHASE
    if(currentGhostMode == GhostMode.CHASE) {
      return GetGhostTypeTargetTile();
    }
    return settings.scatterTile;
  }

  public Vector2Int GetCurrentTile() { return currentTile;}

  Vector2Int GetGhostTypeTargetTile()
  {
    Vector2Int targetTile = new Vector2Int(0,0);

    /*
     * NOTE:  all information below about the schemes comes from the source:
     *  https://www.gamasutra.com/view/feature/3938/the_pacman_dossier.php?print=1
     */
    switch(settings.chaseScheme) {
      case CHASEScheme.TargetPacman:
        /*
         * NOTE:  Blinky's targeting scheme
         *        "Blinky's is the most simple and direct, using Pac-Man's
         *        current tile as his target."
         */

        return pacman.GetCurrentTile();

      case CHASEScheme.AheadOfPacman:
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
        // Pac-Man's direction == Dir.Up

        return maze.GetTileInDirection(pacman.GetCurrentTile(),
          pacman.currentDir, 4, true);

      case CHASEScheme.Collaborate:
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
      case CHASEScheme.CircleAround:
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
          "CHASEScheme not found.");
    }
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
    if(currentGhostMode != GhostMode.FRIGHTENED) {
      if(newGhostMode == GhostMode.FRIGHTENED) {
        // switch to scared animation controller
        animator.runtimeAnimatorController = scaredAnimatorController;
      }
      SwitchDirection(ref currentMove);
    } else {
      // current mode is frightened, new mode not --> animation back to normal
      animator.runtimeAnimatorController = regularAnimatorController;
    }





    // cache the new ghost mode
    currentGhostMode = newGhostMode;

    ResetMoves(currentMove);
  }

  void SwitchDirection(ref Move move) {
    move.direction = oppositeDirs[(int) move.direction];
  }

  void SetDir(Maze.Dir dir) {
    if(dir != currentDir) {
      animator.SetInteger("direction", (int) dir);
      currentDir = dir;
    }

  }

}
}
