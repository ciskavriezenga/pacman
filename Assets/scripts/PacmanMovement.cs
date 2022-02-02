using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO - use with Vector2Int instead of TileCoord
public class PacmanMovement : MonoBehaviour
{
  // current position of pacman, also used as start position
  public Vector2 currentPos = new Vector2(13.875f, 7.625f);
  // speed of pacman
  public float speed;
  // reference to the grid object
  public Grid grid;

  // tile in the pacman maze grid
  private TileCoordinate currentTile;
  // current movement directions
  private Grid.Dir currentDir = Grid.Dir.Left;
  // target position
  private Vector2 targetPos;

  // Start is called before the first frame update
  void Start()
  {
    // fetch direct reference to grid object
    grid = grid.GetComponent<Grid>();
    // set the current tile based on current position
    currentTile = grid.GetTileCoordinate(currentPos);
    Debug.Log("Pacman start tile: " + currentTile.x + " " + currentTile.y);
    // set new target position
    SetNewTargetPos();
  }

  // Update is called once per frame
  // no use of physics, so using Update instead of FixedUpdate for now
  void Update()
  {
    // NOTE:
    // - if multipe keys are pressed, left is favorised to down etc.
    // - instead of caching the direction to press and only once check for validity
    //   we check for each direction. E.g. when up and left are pressed and
    //   heading left is not allowed but up is, we change our direction upwards

    // TODO - prevent multiple key presses 2 directions issue
    if (Input.GetKey(KeyCode.UpArrow)) {
      if(ChangeDir(Grid.Dir.Up)) return;
    }
    if (Input.GetKey(KeyCode.RightArrow)) {
      if(ChangeDir(Grid.Dir.Right)) return;
    }
    if (Input.GetKey(KeyCode.DownArrow)) {
      if(ChangeDir(Grid.Dir.Down)) return;
    }
    if(Input.GetKey(KeyCode.LeftArrow)) {
      if(ChangeDir(Grid.Dir.Left)) return;
    }
  }

  void FixedUpdate()
  {
    currentPos = Vector2.MoveTowards(currentPos, targetPos, speed);
    // transform to view grid and pixelate
    transform.position = grid.SnapToPixel(currentPos);

    // Debug.Log("currentPos: " + currentPos.x + " " + currentPos.y);
    // Debug.Log("pixelctPos: " + transform.position.x + " " + transform.position.y);
    // Debug.Log("targetPos: " + targetPos.x + " " + targetPos.y);

    // if tile changed, update current tile and set new target position
    TileCoordinate tileCoord = grid.GetTileCoordinate(transform.position);
    if(currentTile.Differs(tileCoord)) {
      currentTile = tileCoord;
      SetNewTargetPos();
    }
  }

  void SetNewTargetPos()
  {
    // retrieve target tile based on current tile and current direction
    TileCoordinate targetTile = grid.GetTargetTile(currentTile, currentDir);
    // check if target tile is valid
    // if valid: retrieve target position in the target tile
    // else: set target position to current tile center
    if(grid.TileIsPath(targetTile)) {
      targetPos = grid.GetTargetPos(targetTile, currentDir);
    } else {
      targetPos = grid.GetTileCenterPos(currentTile);
    }
  }

  bool ChangeDir(Grid.Dir dir)
  {
    // no need to change if the currentDirection is the same
    if(currentDir != dir) {
      TileCoordinate targetTile = grid.GetTargetTile(currentTile, dir);
      if(grid.TileIsPath(targetTile)) {
        // store the new direction in currentDir to new dir
        currentDir = dir;
        // update target tile and position
        targetPos = grid.GetTargetPos(targetTile, currentDir);
        return true;
      }
    }

    return false;
  }

}
