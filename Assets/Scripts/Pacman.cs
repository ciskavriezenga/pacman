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
    private float speed;
    // reference to the grid object
    private Maze maze;
    // tile in the pacman maze grid
    [SerializeField] private Vector2Int currentTile;
    // current movement directions
    [SerializeField] public Maze.Dir currentDir {get; private set;}
    // target position
    [SerializeField] private Vector2 moveToPos;

    // --------------- Graphics and UI ---------------------------------------
    // animator reference
    private Animator animator;
    private Maze.Dir lastHitKeyDir = Maze.Dir.None;

    public void Initialize(PacmanSettings settings) {
      // get reference to animator
      animator = GetComponent<Animator>();

      // set start values
      currentPos = settings.startPos;
      SetDir(settings.startDirection);
      speed = settings.speed;

      // retrieve reference to the maze
      maze = GameManager.Instance.GetMaze();


      // initialize start values
      currentTile = maze.GetTileCoordinate(currentPos);

      // set new target position
      SetNewMoveToPos();
    }

    // Update is called once per frame
    // no use of physics, so using Update instead of FixedUpdate for now
    void Update()
    {
      // cache the last hit key
      CacheLastHitArrowKey();
      // clear last hit key if the cached key equals a arrow key up event
      ClearLastHitKey();

      // if there is a arrow key is cached, act on it
      if(lastHitKeyDir != Maze.Dir.None) {
        if(ChangeDir(lastHitKeyDir)) {
          // we changed the direction, clear the key cache
          lastHitKeyDir = Maze.Dir.None;
        }
      }
    }

    // caches the last hit key to lastHitKeyDir class member
    void CacheLastHitArrowKey() {
      if (Input.GetKeyDown(KeyCode.UpArrow)) {
        lastHitKeyDir = Maze.Dir.Up;
      }
      if (Input.GetKeyDown(KeyCode.RightArrow)) {
        lastHitKeyDir = Maze.Dir.Right;
      }
      if (Input.GetKeyDown(KeyCode.DownArrow)) {
        lastHitKeyDir = Maze.Dir.Down;
      }
      if(Input.GetKeyDown(KeyCode.LeftArrow)) {
        lastHitKeyDir = Maze.Dir.Left;
      }
    }

    // clears the cache of the last hit key if key up event equals the last hit key
    void ClearLastHitKey() {
      if (Input.GetKeyUp(KeyCode.UpArrow) && lastHitKeyDir == Maze.Dir.Up
      || Input.GetKeyUp(KeyCode.RightArrow) && lastHitKeyDir == Maze.Dir.Right
      || Input.GetKeyUp(KeyCode.DownArrow) && lastHitKeyDir == Maze.Dir.Down
      || Input.GetKeyUp(KeyCode.LeftArrow) && lastHitKeyDir == Maze.Dir.Left) {
        lastHitKeyDir = Maze.Dir.None;
      }
    }

    void FixedUpdate()
    {
      currentPos = Vector2.MoveTowards(currentPos, moveToPos, speed);
      // transform to view grid and pixelate
      transform.position = maze.SnapToPixel(currentPos);

      // if tile changed, update current tile and set new target position
      Vector2Int newTile = maze.GetTileCoordinate(transform.position);

      if(!currentTile.Equals(newTile)) {
        currentTile = newTile;
        // if this new tile is a teleport tile --> teleport :D
        if(maze.TileIsTeleport(currentTile)) {
          Teleport();
        }
        SetNewMoveToPos();
      }
    }

    void SetNewMoveToPos()
    {
      // retrieve target tile based on current tile and current direction
      Vector2Int moveTile = maze.GetAdjacentTile(currentTile, currentDir);

      // check if target tile is valid
      // if valid: retrieve target position in the target tile
      // else: set target position to current tile center
      if(maze.TileIsPath(moveTile)) {
        moveToPos = maze.GetMoveToPos(moveTile, currentDir);
      } else {
        moveToPos = maze.GetCenterPos(currentTile);
      }
    }

    bool ChangeDir(Maze.Dir dir)
    {
      // no need to change if the currentDirection is the same
      if(currentDir != dir) {
        Vector2Int moveTile = maze.GetAdjacentTile(currentTile, dir);
        if(maze.TileIsPath(moveTile)) {
          // store the new direction in currentDir to new dir
          SetDir(dir);
          // update move to position
          moveToPos = maze.GetMoveToPos(moveTile, currentDir);
          return true;
        }
      }

      return false;
    }

    void Teleport()
    {
      // get next adjacent tile and wrap it
      currentTile = maze.GetAdjacentTile(currentTile, currentDir);
      maze.WrapTile(ref currentTile);
      // reset current position to new tile position
      currentPos = maze.GetMoveToPos(currentTile, currentDir);
      // transform to view grid and pixelate
      transform.position = maze.SnapToPixel(currentPos);
    }

    void SetDir(Maze.Dir dir)
    {
      currentDir = dir;
      animator.SetInteger("direction", (int) dir);
    }

    public Vector2Int GetCurrentTile() {
      return currentTile;
    }
  }
}
