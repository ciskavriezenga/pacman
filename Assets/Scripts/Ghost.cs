#define SHOW_GHOST_TARGET_TILE
//#define DEBUG_GHOSTMODE
//#define DEBUG_MOVEMENT
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
#if DEBUG_MOVEMENT
    if(name == "blinky") {
      Debug.Log("curPos: " + curPos
      + " distanceToTarget: " + distanceToTarget
      + " curMove.Pos: " + curMove.pos
      + " curDir: " + curDir
      + " curMove.dir: " + curMove.dir
      );
    }
#endif

    if(distanceToTarget < moveDistance) {
      // we will either reached center new movement axis or target
      if(curDir == curMove.dir) {
        Move(distanceToTarget);
        moveDistance -= distanceToTarget;
#if DEBUG_MOVEMENT
        if(name == "blinky") {
          Debug.Log("REACHED TARGET");
        }
#endif
        // reached target tile
        curTile = curMove.tile;
        // TODO - take difference in speed and leftover movement into account?
        ProcessCurrentTileType();
        curMove = nextMove;
        nextMove = GenerateMove(curMove.tile, curMove.dir);
        // in case opposite --> immediately change direction
        if(curMove.dir == oppositeDirs[(int) curDir]){
          curDir = curMove.dir;
          animator.SetInteger("direction", (int) curDir);
        }
      } else {
#if DEBUG_MOVEMENT
        if(name == "blinky") {
          Debug.Log("REACHED CENTER");
        }
#endif
        Move(distanceToTarget);
        moveDistance -= distanceToTarget;
        // we reached the center for the new move direction --> new direction
        curDir = curMove.dir;
        // update animation direction
        animator.SetInteger("direction", (int) curDir);
      }
    }
    // either move full step or leftover distance
    Move(moveDistance);
  }

  void Move(float displacement) {
    // move the displacement amount into the current direction
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
    // update GameObject position
    transform.position = curPos;
  }


  float CalcDistanceMoveAxis() {
    // calculate the distance tot the target position on current movement axis
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

  GhostMove GenerateMove(Vector2Int fromTile, Dir direction)
  {
    // return the tile that corresponds to the cur mode
    Vector2Int targetTile = Vector2Int.zero;
    switch(curGhostMode) {
      case GhostMode.CHASE:
        targetTile = GetChaseTargetTile();
        break;
      case GhostMode.SCATTER:
        targetTile = settings.scatterTile;
        break;
      case GhostMode.FRIGHTENED:
        return GenerateFrightenedMove(fromTile, direction);
      case GhostMode.PACING_HOME:
        return GeneratePacingHomeMove(fromTile, direction);
      case GhostMode.LEAVING_HOME:
        targetTile = maze.ghostDoorTargetTile;
#if DEBUG_GHOSTMODE
        Debug.Log("LEAVING_HOME - targetTile: " + targetTile);
#endif
        if(curTile == targetTile) {
          curGhostMode = gameManager.curGhostMode;
          return GenerateMove(fromTile, direction);
        }
        break;
    }
    #if SHOW_GHOST_TARGET_TILE
        targetTileSR.transform.position = maze.GetCenterPos(targetTile);
    #endif
    return CreateTargetedMove(fromTile, direction, targetTile);
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
    // NOTE - hardcoded - only horizontal doors now
    return CreateGhostMove(targetTile, direction, 0.375f);
  }

  private GhostMove GenerateFrightenedMove(Vector2Int fromTile, Dir direction)
  {
    // random direction + test other options in order: up, left, down, and right
    Dir dir = (Dir) (Random.value * 3.999999999f);
    Vector2Int randomTile = maze.GetAdjacentTile(fromTile, dir);
    // NOTE: up movement on no-upmovement tiles is allowed while frightened
    int numTimes = 0;
    // TODO - retrieve open directions for current tile instead of TileIsPath
    // see pacman.js
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
    return CreateGhostMove(randomTile, dir);
  }

  private GhostMove CreateTargetedMove(Vector2Int fromTile, Dir direction, Vector2Int targetTile)
  {
    // retrieve adjacentTiles
    Dir[] directions = allowedDirs[(int) direction];
    Vector2Int[] adjacentTiles = GetAdjacentTiles(fromTile, directions);

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

    // return new GhostMove
    float offsetX = 0;
    // add offset if ghost is leaving home
    if(curGhostMode == GhostMode.LEAVING_HOME) {
      // NOTE - currently hardcoded - no vertical doors possible
      offsetX  = 0.375f;
    }
    return CreateGhostMove(adjacentTiles[indexBestMove], directions[indexBestMove], offsetX);
  }

  GhostMove CreateGhostMove(Vector2Int tile, Dir dir, float offsetX = 0) {
    // retrieve targetPos
    Vector2 targetPos = maze.GetMoveToPos(tile, dir);
    // add offset
    if(offsetX > 0.01 | offsetX < -0.01) targetPos.x += offsetX;
    // create new ghostmove
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
    // NOTE : currently only horizontal teleport functionality

    // deltaX is positive when curTile.x == 0
    int deltaX = maze.width - 1;
     // deltaX is negative -
    if(curTile.x != 0) deltaX *= -1;

    curTile = curTile + new Vector2Int(deltaX, 0);
    curPos.x += deltaX;
    transform.position = curPos;

    curMove = GenerateMove(curTile, curMove.dir);
    nextMove = GenerateMove(curMove.tile, curMove.dir);
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
#if DEBUG_GHOSTMODE
    if(name == "blinky") {
      Debug.Log("Ghost.SwitchMode - currentMode: " + curGhostMode
      + ", new mode: " + newGhostMode );
    }
#endif
    // do nothing if new mode equals cur
    if(newGhostMode == curGhostMode) {
      return;
    }

    // Do NOT change mode if the ghost is waiting in the ghosthouse
    if(curGhostMode != GhostMode.PACING_HOME
      && curGhostMode != GhostMode.LEAVING_HOME) {

      GhostMode fromGhostMode = curGhostMode;
      curGhostMode = newGhostMode;
      // Ghosts do not reverse direction when leaving frigthened mode
      if(fromGhostMode == GhostMode.FRIGHTENED) {
        // cur mode is frightened, new mode not --> animation back to normal
        animator.runtimeAnimatorController = regularAnimatorController;
        // fetch new next move
        nextMove = GenerateMove(curMove.tile, curMove.dir);
      } else {
        if(newGhostMode == GhostMode.FRIGHTENED) {
          // switch to scared animation controller
          animator.runtimeAnimatorController = scaredAnimatorController;
        }
        Dir newDir = oppositeDirs[(int) curMove.dir];
        // fetch new next move
        Vector2Int tile = maze.GetAdjacentTile(curMove.tile, newDir);
        nextMove = CreateGhostMove(tile, newDir);
      }

#if DEBUG_GHOSTMODE
      if(name == "blinky") {
        Debug.Log("Ghost.SwitchMode - new mode: " + newGhostMode + " ***");
        Debug.Log(".................. curMove.dir: " + curMove.dir
        + ", nextMove.dir: " + nextMove.dir);
        Debug.Log(".................. curMove.tile: " + curMove.tile
        + ", nextMove.tile: " + nextMove.tile);
      }
#endif

    }
  }
  public void LeaveHome() {
    // be sure the current mode is pacing home
    if(curGhostMode == GhostMode.PACING_HOME) {
      curGhostMode = GhostMode.LEAVING_HOME;
    } else {
      Debug.Log(" **** ERROR ***** - this should not happen - Ghost.LeaveHome - name: "
      + name);
    }
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

  public Vector2Int GetCurTile() { return curTile;}

}

}
