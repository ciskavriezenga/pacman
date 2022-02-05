using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO - use with Vector2Int instead of Coordinate
public class PacmanMovement : MonoBehaviour
{
  // current position of pacman, also used as start position
  public Vector2 currentPos = new Vector2(13.875f, 7.625f);
  // speed of pacman
  public float speed;
  // reference to the grid object
  public Grid grid;

  // TODO - make private and add getter
  // tile in the pacman maze grid
  public Vector2Int currentTile;
  // current movement directions
  private Grid.Dir currentDir = Grid.Dir.Left;
  // target position
  private Vector2 moveToPos;
  private Grid.Dir lastHitKeyDir = Grid.Dir.None;

  // Start is called before the first frame update
  void Start()
  {
    // fetch direct reference to grid object
    grid = grid.GetComponent<Grid>();
    // set the current tile based on current position
    currentTile = grid.GetTileCoordinate(currentPos);
    Debug.Log("Pacman start tile: " + currentTile.x + " " + currentTile.y);
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
    if(lastHitKeyDir != Grid.Dir.None) {
      if(ChangeDir(lastHitKeyDir)) {
        // we changed the direction, clear the key cache
        lastHitKeyDir = Grid.Dir.None;
      }
    }
  }

  // caches the last hit key to lastHitKeyDir class member
  void CacheLastHitArrowKey() {
    if (Input.GetKeyDown(KeyCode.UpArrow)) {
      lastHitKeyDir = Grid.Dir.Up;
    }
    if (Input.GetKeyDown(KeyCode.RightArrow)) {
      lastHitKeyDir = Grid.Dir.Right;
    }
    if (Input.GetKeyDown(KeyCode.DownArrow)) {
      lastHitKeyDir = Grid.Dir.Down;
    }
    if(Input.GetKeyDown(KeyCode.LeftArrow)) {
      lastHitKeyDir = Grid.Dir.Left;
    }
  }

  // clears the cache of the last hit key if key up event equals the last hit key
  void ClearLastHitKey() {
    if (Input.GetKeyUp(KeyCode.UpArrow) && lastHitKeyDir == Grid.Dir.Up
    || Input.GetKeyUp(KeyCode.RightArrow) && lastHitKeyDir == Grid.Dir.Right
    || Input.GetKeyUp(KeyCode.DownArrow) && lastHitKeyDir == Grid.Dir.Down
    || Input.GetKeyUp(KeyCode.LeftArrow) && lastHitKeyDir == Grid.Dir.Left) {
      lastHitKeyDir = Grid.Dir.None;
    }
  }

  void FixedUpdate()
  {
    currentPos = Vector2.MoveTowards(currentPos, moveToPos, speed);
    // transform to view grid and pixelate
    transform.position = grid.SnapToPixel(currentPos);

    // if tile changed, update current tile and set new target position
    Vector2Int tileCoord = grid.GetTileCoordinate(transform.position);

    if(!currentTile.Equals(tileCoord)) {
      currentTile = tileCoord;
      SetNewMoveToPos();
    }
  }

  void SetNewMoveToPos()
  {
    // retrieve target tile based on current tile and current direction
    Vector2Int targetTile = grid.GetAdjacentTile(currentTile, currentDir);
    // check if target tile is valid
    // if valid: retrieve target position in the target tile
    // else: set target position to current tile center
    if(grid.TileIsPath(targetTile)) {
      moveToPos = grid.GetMoveToPos(targetTile, currentDir);
    } else {
      moveToPos = grid.GetCenterPos(currentTile);
    }
  }

  bool ChangeDir(Grid.Dir dir)
  {
    // no need to change if the currentDirection is the same
    if(currentDir != dir) {
      Vector2Int targetTile = grid.GetAdjacentTile(currentTile, dir);
      if(grid.TileIsPath(targetTile)) {
        // store the new direction in currentDir to new dir
        currentDir = dir;
        // update move to position
        moveToPos = grid.GetMoveToPos(targetTile, currentDir);
        return true;
      }
    }

    return false;
  }

}
