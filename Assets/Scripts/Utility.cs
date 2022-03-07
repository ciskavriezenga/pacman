using UnityEngine;

namespace PM {

// ==============================================================================
// =============== dinstance utility methods ===================================
// ==============================================================================
public static class Utility {
  // squared distance - integer
  public static int SquaredEuclideanDistance(Vector2Int tile1, Vector2Int tile2)
  {
    int a = tile2.x - tile1.x;
    int b = tile2.y - tile1.y;
    return a * a + b * b;
  }

  // squared distance - float
  public static float SquaredEuclideanDistance(Vector2 pos1, Vector2 pos2)
  {
    float a = pos2.x - pos1.x;
    float b = pos2.y - pos1.y;
    return a * a + b * b;
  }

  // ManhattanDistance - integer
  public static int ManhattanDistance(Vector2Int tile1, Vector2Int tile2)
  {
    int a = tile2.x - tile1.x;
    int b = tile2.y - tile1.y;
    return System.Math.Abs(a) + System.Math.Abs(b);
  }

  // ManhattanDistance - float
  public static float ManhattanDistance(Vector2 pos1, Vector2 pos2)
  {
    float a = pos2.x - pos1.x;
    float b = pos2.y - pos1.y;
    return System.Math.Abs(a) + System.Math.Abs(b);
  }

} // end class

} // end namespace
