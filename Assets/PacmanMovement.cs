using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PacmanMovement : MonoBehaviour
{
  public Vector2 currentPos = new Vector2(13.875f, 7.625f);
  public float speed;
  public Grid grid;

  // target position
  private Vector2 targetPos;

  // tile in the pacman maze grid
  private TileCoordinate currentTile;
  private TileCoordinate targetTile;

  // current movement directions
  private Grid.Dir currentDir = Grid.Dir.Left;
  private int counter = 0;

  // Start is called before the first frame update
  void Start()
  {
    grid = grid.GetComponent<Grid>();
    currentTile = grid.GetTileCoordinate(currentPos);
    SetNewTargetPos();
  }



  // Update is called once per frame
  // no use of physics, so using Update instead of FixedUpdate for now
  void Update()
  {
    ProcessKeyPress();
  }

  void FixedUpdate()
  {
    currentPos = Vector2.MoveTowards(currentPos, targetPos, speed);
    // transform to view grid and pixelate
    transform.position = grid.SnapToPixel(currentPos);

    Debug.Log("currentPos: " + currentPos.x + " " + currentPos.y);
    Debug.Log("pixelctPos: " + transform.position.x + " " + transform.position.y);
    // if tile changed, update target position
    // TODO - add track change to TileCoord struct
    TileCoordinate tileCoord = grid.GetTileCoordinate(transform.position);
    if(currentTile.Differs(tileCoord)) {
      currentTile = tileCoord;
      SetNewTargetPos();
    }
    // if(counter % 100 == 0 ) {
    //   Debug.Log("tileCoord: " + tileCoord.x + " " + tileCoord.y);
    //   Debug.Log("currentTile: " + currentTile.x + " " + currentTile.y);
    //   Debug.Log("targetTile: " + targetTile.x + " " + targetTile.y);
    //   Debug.Log("differs: " + currentTile.differs(tileCoord));
    //   Debug.Log("current position pixalized: " + transform.position.x + ", " + transform.position.y);
    //   Debug.Log("target position pixalized: " + targetPos.x + ", " + targetPos.y);
    // }
    // counter++;
  }

  void ProcessKeyPress()
  {
    // NOTE:
    // - if multipe keys are pressed, left is favorised to down etc.
    // - instead of caching the direction to press and only once check for validity
    //   we check for each direction. E.g. when up and left are pressed and
    //   heading left is not allowed but up is, we change our direction upwards

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

  void SetNewTargetPos()
  {
    TileCoordinate newTargetTile = grid.GetTargetTile(currentTile, currentDir);
    Debug.Log("currentTile: " + currentTile.x + " " + currentTile.y);
    Debug.Log("targetTile: " + targetTile.x + " " + targetTile.y);
    if(grid.TileIsPath(newTargetTile)) {
      targetPos = grid.GetTargetPos(newTargetTile, currentDir);
    } else {
      targetPos = grid.GetTileCenterPos(currentTile);
    }
  }

  bool ChangeDir(Grid.Dir dir)
  {
    Debug.Log("ChangeDir - top");
    // no need to change if the currentDirection is the same
    if(currentDir != dir) {
      Debug.Log("current dir is not the same as new dir");
      TileCoordinate newTargetTile = grid.GetTargetTile(currentTile, dir);
      newTargetTile.Log("new target tile");
      if(grid.TileIsPath(newTargetTile)) {
        // store the new direction in currentDir to new dir
        currentDir = dir;
        // update target tile and position
        targetPos = grid.GetTargetPos(newTargetTile, currentDir);
        return true;
      }
    }

    return false;
  }

}





/*

https://stackoverflow.com/questions/34447682/what-is-the-difference-between-update-fixedupdate-in-unity
Update()

Called every frame
Used for regular updates such as :
Moving non-physics objects
Simple timers
Receiving input (aka keypress etc)
Update interval call times will vary, ie non-uniformly spaced
FixedUpdate()

Called every physics step
FixedUpdate() intervals are consistent, ie uniformly spaced
Used for regular updates such as adjusting physic (eg. RigidBody) objects

*/
