using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM {
  public class Pacman : MonoBehaviour
  {
    // -------------- model: position, tile, direction, target ---------------
    // current position of pacman, also used as start position
    [SerializeField] public Vector2 currentPos {get; private set;}
    // speed of pacman
    public float speed;
    // reference to the grid object
    [SerializeField] private GameManager gameManager;
    // reference to the grid object
    [SerializeField] private Maze maze;
    private PacmanSettings settings;
    // tile in the pacman maze grid
    [SerializeField] private Vector2Int curTile;
    // current movement directions
    [SerializeField] public Dir curDir {get; private set;}
    // target position
    [SerializeField] private Vector2 moveToPos;

    // --------------- Graphics and UI ---------------------------------------
    // animator reference
    private Animator animator;
    private Dir lastHitKeyDir = Dir.NONE;

    public void Initialize(PacmanSettings settings, GameManager gameManager) {
      this.settings = settings;
      // retrieve reference to the maze
      this.gameManager = gameManager;
      this.maze = gameManager.GetMaze();      

      // get reference to animator
      animator = GetComponent<Animator>();

      // set start values
      currentPos = settings.startPos;
      SetDir(settings.startDirection);
      speed = settings.normSpeed;



      // initialize start values
      curTile = maze.GetTileCoordinate(currentPos);

      // set new target position
      SetNewMoveToPos();
    }

    // Update is called once per frame
    // no use of physics, so using Update instead of FixedUpdate for now
    void Update()
    {
      // we want to enable to act on the latest pressed arrow key
      // cache the last hit key
      CacheLastHitArrowKey();
      // clear last hit key if the cached key equals a arrow key up event
      ClearLastHitKey();

      // if there is an arrow key cached, act on it
      if(lastHitKeyDir != Dir.NONE) {
        if(ChangeDir(lastHitKeyDir)) {
          // we changed the direction, clear the key cache
          lastHitKeyDir = Dir.NONE;
        }
      }
    }

    // caches the last hit key to lastHitKeyDir class member
    void CacheLastHitArrowKey() {
      if (Input.GetKeyDown(KeyCode.UpArrow)) {
        lastHitKeyDir = Dir.UP;
      }
      if (Input.GetKeyDown(KeyCode.RightArrow)) {
        lastHitKeyDir = Dir.RIGHT;
      }
      if (Input.GetKeyDown(KeyCode.DownArrow)) {
        lastHitKeyDir = Dir.DOWN;
      }
      if(Input.GetKeyDown(KeyCode.LeftArrow)) {
        lastHitKeyDir = Dir.LEFT;
      }
    }

    // clears the cache of the last hit key if key up event equals the last hit key
    void ClearLastHitKey() {
      if (Input.GetKeyUp(KeyCode.UpArrow) && lastHitKeyDir == Dir.UP
      || Input.GetKeyUp(KeyCode.RightArrow) && lastHitKeyDir == Dir.RIGHT
      || Input.GetKeyUp(KeyCode.DownArrow) && lastHitKeyDir == Dir.DOWN
      || Input.GetKeyUp(KeyCode.LeftArrow) && lastHitKeyDir == Dir.LEFT) {
        lastHitKeyDir = Dir.NONE;
      }
    }

    void FixedUpdate()
    {
      currentPos = Vector2.MoveTowards(currentPos, moveToPos, speed);

      // transform to view grid and pixelate
      transform.position = maze.SnapToPixel(currentPos);

      // if tile changed, update current tile and set new target position
      Vector2Int newTile = maze.GetTileCoordinate(transform.position);

      if(!curTile.Equals(newTile)) {
        curTile = newTile;

        // update current speed
        if(gameManager.PacmanEatsPellet(curTile)) {
          if(gameManager.GameModeIsFrightened()) {
            speed = settings.frightDotSpeed;
          } else {
            speed = settings.normDotSpeed;
          }
        } else {
          if(gameManager.GameModeIsFrightened()) {
            speed = settings.frightSpeed;
          } else {
            speed = settings.normSpeed;
          }
        }

        // if this new tile is a teleport tile --> teleport :D
        if(maze.TileIsTeleport(curTile)) {
          Teleport();
        }
        SetNewMoveToPos();
      }
    }

    void SetNewMoveToPos()
    {
      // retrieve target tile based on current tile and current direction
      Vector2Int moveTile = maze.GetAdjacentTile(curTile, curDir);

      // check if target tile is valid
      // if valid: retrieve target position in the target tile
      // else: set target position to current tile center
      if(maze.TileIsPath(moveTile)) {
        moveToPos = maze.GetMoveToPos(moveTile, curDir);
      } else {
        moveToPos = maze.GetCenterPos(curTile);
      }
    }

    bool ChangeDir(Dir dir)
    {
      // no need to change if the curDirection is the same
      if(curDir != dir) {
        Vector2Int moveTile = maze.GetAdjacentTile(curTile, dir);
        if(maze.TileIsPath(moveTile)) {
          // store the new direction in curDir to new dir
          SetDir(dir);
          // update move to position
          moveToPos = maze.GetMoveToPos(moveTile, curDir);
          return true;
        }
      }

      return false;
    }

    void Teleport()
    {
      // get next adjacent tile and wrap it
      curTile = maze.GetAdjacentTile(curTile, curDir);
      maze.WrapTile(ref curTile);
      // reset current position to new tile position
      currentPos = maze.GetMoveToPos(curTile, curDir);
      // transform to view grid and pixelate
      transform.position = maze.SnapToPixel(currentPos);
    }

    void SetDir(Dir dir)
    {
      if(curDir != dir) {
        animator.SetInteger("direction", (int) dir);
        curDir = dir;
      }

    }

    public Vector2Int GetCurTile() { return curTile;}

  }
}
