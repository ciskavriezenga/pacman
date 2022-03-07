#define SHOW_GHOST_TARGET_TILE
#define DEBUG_GHOST

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PM {

public enum GhostMode
{
  CHASE = 0,
  SCATTER = 1,
  FRIGHTENED = 2,
  PACING_HOME = 3,
  LEAVING_HOME = 4
}

public class Ghost : MonoBehaviour
{
  // enum to for the different chase schemes
  // TODO - private?
  public enum ChaseScheme
  {
    TARGET_PACMAN = 0,   // regular Blinky behaviour
    AHEAD_OF_PACMAN = 1,  // regular Pinky behaviour
    COLLABORATE = 2,    // regular Inky behaviour, based on blinky pos and pm
    CIRCLE_AROUND = 3,   // regular Clyde behaviour
  }

  // -- directions and related ------------------------------------------------
  // the oposite directions to a given direction
  private Dir[] oppositeDirs = {
    Dir.LEFT, // Right --> Left
    Dir.UP, // Down --> up
    Dir.RIGHT, // left --> right
    Dir.DOWN // Up --> down
  };

  // 2d array with the allowed directions based on cur direction
  // prefered order up, left, down, right
  private Dir[][] allowedDirs = {
    new Dir[] {Dir.UP, Dir.DOWN, Dir.RIGHT}, // cur = R
    new Dir[] {Dir.LEFT, Dir.DOWN, Dir.RIGHT}, // cur = D
    new Dir[] {Dir.UP, Dir.LEFT, Dir.DOWN}, // cur = L
    new Dir[] {Dir.UP, Dir.LEFT, Dir.RIGHT} // cur = U
  };


  // -- references & settings -------------------------------------------------
  // reference to other objects - necessary to react
  [SerializeField] private GameManager gameManager;
  [SerializeField] private Maze maze;
  [SerializeField] private Pacman pacman;

  // the settings - used for initiate and reset
  private GhostSettings settings;

  // -- current values --------------------------------------------------------
  // cur pos, tile, direction
  [SerializeField] private Vector2 curPos;
  [SerializeField] private Vector2Int curTile;
  [SerializeField] private Dir curDir;
  [SerializeField] private float curSpeed;
  // TODO - use curMoveVector

  // ghost mode, either scatter, frightened, chase
  private GhostMode curGhostMode;

  // -- moves and related values ----------------------------------------------
  // we need to cache the current move and last move
  GhostMove curMove;
  GhostMove nextMove;
  // the ghost that will be used in case of the collaborate scheme
  // which corresponds to the original Inky behavior based on Blinky's pos
  private Ghost wingman;

  // -- debug - target tiles -------------------------------------------------
  #if SHOW_GHOST_TARGET_TILE
    //public Vector2Int scatterPos;
    private GameObject targetTileSR;
    private GameObject scatterTileSR;
  #endif

  // -- animation related fields ----------------------------------------------
  [SerializeField] private Animator animator;
  [SerializeField] private RuntimeAnimatorController regularAnimatorController;
  [SerializeField] private RuntimeAnimatorController scaredAnimatorController;



// =============================================================================
// = Initialize methods ========================================================
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
    // cache the cur ghost mode
    if(settings.startInGhosthouse) {
      curGhostMode = GhostMode.PACING_HOME;
    } else {
      curGhostMode = gameManager.curGhostMode;
    }

    // set initial values
    curTile = maze.GetTileCoordinate(settings.startPos);
    curPos = settings.startPos;
    curSpeed = settings.normSpeed;
    // position ghost to start pos
    transform.position = curPos;

    // init moves
    curMove = GenerateMove(curTile, settings.startDirection);
    nextMove = GenerateMove(curMove.tile, settings.startDirection);
    curDir = curMove.dir;
  }

// =============================================================================
// =Update methods ============================================================
// =============================================================================

  // Update is called once per frame
  void FixedUpdate()
  {
    // move curPos closer towards target position

    float distanceToTarget = CalcDistanceMoveAxis();
    float moveDistance = curSpeed;
    if(name == "blinky") {
      Debug.Log("curPos: " + curPos
      + " distanceToTarget: " + distanceToTarget
      + " curMove.Pos: " + curMove.pos
      + " curDir: " + curDir
      + " curMove.dir: " + curMove.dir
      );
    }

    if(distanceToTarget < moveDistance) {
      // we will either reached center new movement axis or target
      if(curDir == curMove.dir) {
        Move(distanceToTarget);
        moveDistance -= distanceToTarget;
        if(name == "blinky") {
          Debug.Log("REACHED TARGET");
        }
        // reached target tile
        curMove = nextMove;
        nextMove = GenerateMove(curMove.tile, curMove.dir);
      } else {
        if(name == "blinky") {
          Debug.Log("REACHED CENTER");
        }
        Move(distanceToTarget);
        moveDistance -= distanceToTarget;
        // we reached the center for the new move direction --> new direction
        curDir = curMove.dir;
        // update animation direction
        animator.SetInteger("direction", (int) curMove.dir);
      }
    }

    Move(moveDistance);
  }

  void Move(float displacement) {
    switch(curDir) {
      case Dir.RIGHT:
        curPos.x += displacement;
        break;
      case Dir.DOWN:
        curPos.y -= displacement;
        break;
      case Dir.LEFT:
        curPos.x -= displacement;
        break;
      case Dir.UP:
        curPos.y += displacement;
        break;
    }
    transform.position = curPos;
  }


  float CalcDistanceMoveAxis() {
    switch(curDir) {
      case Dir.RIGHT:
        return curMove.pos.x - curPos.x;
      case Dir.DOWN:
        return curPos.y - curMove.pos.y;
      case Dir.LEFT:
        return curPos.x - curMove.pos.x;
      }
    // up
    return curMove.pos.y - curPos.y;
  }

// =============================================================================
// = Generate moves methods ====================================================
// =============================================================================

  void RegenerateMoves(Vector2Int fromTile, Dir direction, int numMoves)
  {
    // for(int i = 0; i < numMoves; i++) {
    //   // cache lastMove generated
    //   GhostMove move = GenerateMove(fromTile, direction);
    //   moves.Enqueue(move);
    //   fromTile = move.targetTile;
    //   direction = move.dir;
    // }
  }

  GhostMove GenerateMove(Vector2Int fromTile, Dir direction)
  {
    switch(curGhostMode) {
      case GhostMode.PACING_HOME:
        return GeneratePacingHomeMove(fromTile, direction);
      case GhostMode.LEAVING_HOME:
        break;
      case GhostMode.FRIGHTENED:
        return GenerateFrightenedMove(fromTile, direction);
    }
    return CreateChaseScatterMove(fromTile, direction);
  }

  private GhostMove GeneratePacingHomeMove(Vector2Int fromTile, Dir direction)
  {
    // only  up / down movement while pacing
    // TODO - remove later on when all goes smooth and this does not happen
    if(direction == Dir.LEFT || direction == Dir.RIGHT) {
      Debug.Log("Ghost.GeneratePacingHomeMove ***** SHOULD NOT HAPPEN ***** ");
    }
    Vector2Int targetTile = maze.GetAdjacentTile(fromTile, direction);
    // if no free spot - change direction
    if(!maze.TileIsPath(targetTile)) {
      direction = oppositeDirs[(int) direction];
      targetTile = maze.GetAdjacentTile(fromTile, direction);
    }
    // create a new Move struct
#if SHOW_GHOST_TARGET_TILE
    targetTileSR.transform.position = maze.GetCenterPos(targetTile);
#endif
    // add 3 pixels horizontal offset so ghost appears at correct spot
    // TODO fix hard code
    Vector2 targetPos = maze.GetMoveToPos(targetTile, direction);
    targetPos.x += 0.375f;
    return new GhostMove(targetTile, direction, maze.PixelCoordinate(targetPos), targetPos);
  }

  private GhostMove GenerateFrightenedMove(Vector2Int fromTile, Dir direction)
  {
    // random direction + test other options in order: up, left, down, and right
    Dir dir = (Dir) (Random.value * 3.999999999f);
    Vector2Int randomTile = maze.GetAdjacentTile(fromTile, dir);
    // NOTE: up movement on no-upmovement tiles is allowed while frightened
    int numTimes = 0;
    while(dir == oppositeDirs[(int) direction] || !maze.TileIsPath(randomTile)) {
      dir = (Dir) ((int)dir - 1);
      // wrap direction if equal NONE  (-1)
      if(dir == Dir.NONE) dir = Dir.UP;
      randomTile = maze.GetAdjacentTile(fromTile, dir);
      // TODO - remove
      if(numTimes > 4) {
        Debug.Log("FRIGHTENED - ******* ERROR ******* should not happen");
        break;
      }
      numTimes++;
    }
    // create a new Move struct
#if SHOW_GHOST_TARGET_TILE
    targetTileSR.transform.position = maze.GetCenterPos(randomTile);
#endif
    Vector2 targetPos = maze.GetMoveToPos(randomTile, dir);
    return new GhostMove(randomTile, dir, maze.PixelCoordinate(targetPos), targetPos);
  }

  private GhostMove CreateChaseScatterMove(Vector2Int fromTile, Dir direction)
  {

    // retrieve adjacentTiles
    Dir[] directions = allowedDirs[(int) direction];
    Vector2Int[] adjacentTiles = GetAdjacentTiles(fromTile, directions);

    // return the tile that corresponds to the cur mode
    Vector2Int targetTile;
    if(curGhostMode == GhostMode.CHASE) {
      targetTile = GetChaseTargetTile();
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
        || maze.TileIsGhostDoor(adjacentTiles[i]) && directions[i] == Dir.UP) {
        // only allow movement up if allowed
        if(directions[i] != Dir.UP || !maze.TileGhostNoUpward(fromTile)) {
          int distance =
            Utility.SquaredEuclideanDistance(targetTile, adjacentTiles[i]);
          if(distance < smallestDistance) {
            smallestDistance = distance;
            indexBestMove = i;
          }
        }
      }
    } // end forloop
    // create a new Move struct
    // NOTE: corresponding pixels are calculated in Move constructor
    if(indexBestMove == -1) {
      if(maze.TileIsGhostHouse(fromTile)) {
        Debug.Log("***** TODO ***** ");
          // swap direction
          //SwitchDirection(ref direction);
          //return CreateChaseScatterMove(fromTile, direction);
      } else {
        Debug.Log("***** SHOULD NOT HAPPEN");
      }
    }
    // return new GhostMove
    Vector2Int tile = adjacentTiles[indexBestMove];
    Dir dir = directions[indexBestMove];
    Vector2 targetPos = maze.GetMoveToPos(tile, dir);
    return new GhostMove(tile, dir, maze.PixelCoordinate(targetPos), targetPos);
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
// = Proces tile type methods ==================================================
// =============================================================================

  // processes the cur tile type if necessary
  void ProcessCurrentTileType() {
    if(maze.TileIsTeleport(curTile)){
      Teleport();
    } // else - update speed
    else if (curGhostMode == GhostMode.FRIGHTENED) {
      // TODO - use setter and only change if new speed & update distance as well
      curSpeed = settings.frightSpeed;
    } else if (maze.TileIsTunnel(curTile)) {
      curSpeed = settings.tunnelSpeed;
    } else {
      curSpeed = settings.normSpeed;
    }
  }

  // teleports the gost to the otherside
  void Teleport(){
    // NOTE : curly only horizontal teleport functionality

    // deltaX is positive when curTile.x == 0
    int deltaX = maze.width - 1;
     // deltaX is negative -
    if(curTile.x != 0) deltaX *= -1;

    curTile = curTile + new Vector2Int(deltaX, 0);
    curPos.x += deltaX;
    transform.position = curPos;

    // TODO - fix this
    Debug.Log("FIX ME PLEASE!!!! ");
  }

// =============================================================================
// = Target tile methods =======================================================
// =============================================================================


  Vector2Int GetChaseTargetTile()
  {
    Vector2Int targetTile = new Vector2Int(0,0);
    // get correct target tile
    switch(settings.chaseScheme) {

      // Blinky
      case ChaseScheme.TARGET_PACMAN:
        // Pac-Man's cur tile as his target.
        return pacman.GetCurTile();

      //Pinky
      case ChaseScheme.AHEAD_OF_PACMAN:
        // offset 4 tiles away from Pac-Man in PM's direction
        // automatically applies extra 4 Left if dir == UP (bug original game)
        return maze.GetTileInDirection(pacman.GetCurTile(),
          pacman.curDir, 4, true);

      // Inky
      case ChaseScheme.COLLABORATE:
        // 2 times the vector from blinky to 2 tiles in front Pacman
        // if Pacman dir == UP --> extra offset to left bug
        // get the tile 2 tiles in front of pacman
        Vector2Int tilesInFrontOfPM = maze.GetTileInDirection(pacman.GetCurTile(),
           pacman.curDir, 2, true);
        // get wingman position (Blinky in normal configuration)
        Vector2Int targetVector = tilesInFrontOfPM - wingman.GetCurTile();
        // double targetVector
        targetVector = targetVector * 2;
        // add to the wingman's cur tile
        return wingman.curTile + targetVector;

      // Clyde
      case ChaseScheme.CIRCLE_AROUND:
        // distance to Pacman > 8 ? pacman's curile : scatterTile
         float distanceToPM  = (curTile - pacman.GetCurTile()).magnitude;
         if(distanceToPM > 8) {
           return pacman.GetCurTile();
         }
         return settings.scatterTile;

      default:
        throw new System.Exception("Ghost.GetGhostTypeTargetTile - " +
          "ChaseScheme not found.");
    } // end switch chaseScheme
  }

// =============================================================================
// = Switch mode & direction ===================================================
// =============================================================================
  public void SwitchMode(GhostMode newGhostMode) {
    // do nothing if new mode equals cur
    if(newGhostMode == curGhostMode) {
      return;
    }

#if DEBUG_GHOST
    Debug.Log("*** Ghost.SwitchMode - new mode: " + newGhostMode + " ***");
#endif

    // direction for the new Move
    Dir dirNewMove = curMove.dir;
    // Do NOT change mode if the ghost is waiting in the ghosthouse
    if(curGhostMode != GhostMode.PACING_HOME
      && curGhostMode != GhostMode.LEAVING_HOME) {
      // Ghosts do not reverse direction when leaving frigthened mode
      if(curGhostMode == GhostMode.FRIGHTENED) {
        // cur mode is frightened, new mode not --> animation back to normal
        animator.runtimeAnimatorController = regularAnimatorController;
      } else {
        if(newGhostMode == GhostMode.FRIGHTENED) {
          // switch to scared animation controller
          animator.runtimeAnimatorController = scaredAnimatorController;
        }
        // switch direction for the new move if current mode !=- frightened
        SwitchDirection(ref dirNewMove);
      }
      // cache the new ghost mode
      curGhostMode = newGhostMode;

      // only generate 1 move, because we can still use the current move
      // depending on the current amount of moves - generate new num of moves
      Debug.Log("FIX ME FIX ME");
    }
  }

  void SwitchDirection(ref Dir dir)
  {
    dir = oppositeDirs[(int) dir];
  }


// =============================================================================
// = setters & getter ==========================================================
// =============================================================================
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

  void SetSpeed(float speed) {

  }

  public Vector2Int GetCurTile() { return curTile;}

}

}
