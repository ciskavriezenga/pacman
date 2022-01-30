using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PacmanMovement : MonoBehaviour
{
  public Vector3 currentPos = new Vector3(13.875f, 7.625f, 0.0f);
  public Vector2 speed;
  public Grid grid;

  // tile in the pacman maze grid
  private Vector2 currentTile;
  private Vector2 targetTile;

  // current movement directions
  private byte currentDir = Dir.Left;

  // TODO - maybe move to interface class Movement? or utility movement class?
  enum Dir
  {
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
  }

  // Start is called before the first frame update
  void Start()
  {
    currentTile = grid.GetTileCoordinate(subGridPos);

    // TODO -move to correct pos
    TilePos aimedTile = GetTargetTile(currentTile, currentDir);
    bool validTile = TileIsPath(TilePos aimedTile, Dir currentDir);
    if(validTile) {

    }

    targetPos = grid.GetTargetPos(currentTile, currentDir);
  }



  // Update is called once per frame
  // no use of physics, so using Update instead of FixedUpdate for now
  void Update()
  {
    //ProcessKeyPress();
  }

  void FixedUpdate()
  {
    currentPos = Vector2.Lerp(currentPos, targetPos, speed);
    // transform to view grid and pixelate
    transform.position = grid.SnapToPixel(currentPos);
  }

  void ProcessKeyPress()
  {
    // NOTE:
    // - if multipe keys are pressed, left is favorised to down etc.
    // - instead of caching the direction to press and only once check for validity
    //   we check for each direction. E.g. when up and left are pressed and
    //   heading left is not allowed but up is, we change our direction upwards

    if (Input.GetKey(KeyCode.UpArrow)) {
      if(ChangeDirIfValid((byte) Dir.Up)) return;
    }
    if (Input.GetKey(KeyCode.RightArrow)) {
      if(ChangeDirIfValid((byte) Dir.Right)) return;
    }
    if (Input.GetKey(KeyCode.DownArrow)) {
      if(ChangeDirIfValid((byte) Dir.Down)) return;
    }
    if(Input.GetKey(KeyCode.LeftArrow)) {
      if(ChangeDirIfValid((byte) Dir.Left)) return;
    }

  }

  bool ChangeDirIfValid(byte dir)
  {

    currentDir = dir;
    return true;
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
