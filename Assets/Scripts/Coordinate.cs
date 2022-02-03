using UnityEngine;

public struct Coordinate
{
  private int _x { get; set; }
  private int _y { get; set; }


  public int x
  {
    get { return _x; }
    set{
      _x = value;
    }
  }

  public int y
  {
      get { return _y; }
      set{
        _y = value;
      }
  }

  public Coordinate(int x, int y) {
    _x = x;
    _y = y;
  }

  // ------------ add methods ------------
  public void Add(Coordinate pos)
  {
    _x = _x + pos.x;
    _y = _y + pos.y;
  }
  public Vector2 Add(Vector2 pos)
  {
    return new Vector2((float)_x + pos.x, (float)_y + pos.y);
  }
  public Vector2 Add(float x, float y)
  {
    return new Vector2((float)_x + x, (float)_y + y);
  }

  // ------------ comparison methods ------------
  public bool Equals(Coordinate pos){
    return _x == pos.x && _y == pos.y;
  }

  public bool Differs(Coordinate pos){
    return _x != pos.x || _y != pos.y;
  }

  // ------------ debug methods ------------
  public void Log(string name) {
    Debug.Log(name + ": " + _x + ", " + _y);
  }

}
