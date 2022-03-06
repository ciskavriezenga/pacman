using System.Collections.Generic;
using UnityEngine;


namespace PM {


// =============================================================================
// =============== MazeSettings ================================================
// =============================================================================
public struct MazeSettings {
  public int width;
  public int height;
  public string imgMazePath;
  public string imgMazeGhostZones;
  public string imgMazePellets;
  public string imgGhostHouseTiles;

  public MazeSettings(string imgMazePath, string imgMazeGhostZones,
    string imgMazePellets, string imgGhostHouseTiles)
  {
    width = 32;
    height = 35;
    this.imgMazePath = imgMazePath;
    this.imgMazeGhostZones = imgMazeGhostZones;
    this.imgMazePellets = imgMazePellets;
    this.imgGhostHouseTiles = imgGhostHouseTiles;
  }
}

// =============================================================================
// =============== PacmanSettings ==============================================
// =============================================================================
public struct PacmanSettings {
  // speed
  public float normSpeed;
  public float normDotSpeed;
  public float frightSpeed;
  public float frightDotSpeed;
  // position and direction
  public Vector2 startPos;
  public Maze.Dir startDirection;
  // info
  public string settingsName;

  public PacmanSettings(
    float overallSpeed,
    float normSpeedPerc, float normDotSpeedPerc,
    float frightSpeedPerc, float frightDotSpeedPerc,
    Vector2 startPos, Maze.Dir startDirection,
    string settingsName)
  {
    // speed
    normSpeed = overallSpeed * normSpeedPerc;
    normDotSpeed = overallSpeed * normDotSpeedPerc;
    frightSpeed = overallSpeed * frightSpeedPerc;
    frightDotSpeed = overallSpeed * frightDotSpeedPerc;
    // position and direction
    this.startPos = startPos;
    this.startDirection = startDirection;
    // info
    this.settingsName = settingsName;
  }
}

// =============================================================================
// =============== GhostModeInterval ===========================================
// =============================================================================
public struct GhostModeInterval {
  public GhostMode mode {get; private set;}
  public int interval {get; private set;}

  public GhostModeInterval(GhostMode mode, int interval)
  {
    this.mode = mode;
    this.interval = interval;
  }
}

// =============================================================================
// =============== GhostSettings================================================
// =============================================================================
public struct GhostSettings {
  // position and direction
  public Vector2Int startTile;
  public Maze.Dir startDirection;
  // speed
  public float normSpeed;
  public float frightSpeed;
  public float tunnelSpeed;
  // path finding fields
  public Ghost.CHASEScheme chaseScheme;
  public Vector2Int scatterTile;
  // info
  public Color color;
  public string name;

  public GhostSettings(
    // position and direction
    Vector2Int startTile, Maze.Dir startDirection,
    // speed
    float normSpeed, float frightSpeed, float tunnelSpeed,
    // path finding fields
    Ghost.CHASEScheme chaseScheme, Vector2Int scatterTile,
    // info
    Color color, string name)
  {
    // position and direction
    this.startTile = startTile;
    this.startDirection = startDirection;
    // speed
    this.normSpeed = normSpeed;
    this.frightSpeed = frightSpeed;
    this.tunnelSpeed = tunnelSpeed;
    // path finding fields
    this.chaseScheme = chaseScheme;
    this.scatterTile = scatterTile;
    // info
    this.color = color;
    this.name = name;
  }
}

// =============================================================================
// =============== static GameSettings class ===================================
// =============================================================================


// TODO - create an array with arrays for scatter / chase mode per (multiple)
//        levels according to pdf pacman dosier
public static class GameSettings {
  private static float overallSpeed = 0.1875f;

  public static PacmanSettings GetPacmanSettings()
  {

    return new PacmanSettings(
    // speed: overall speed, percentages: norm, normDot, fright, frightDot
    overallSpeed, 0.8f, 0.71f, 0.9f, 0.79f,
    // start position and direction
    new Vector2(16f, 9.5f), Maze.Dir.Left,
    // info - name
    "default-pacman-settings");
  }

  public static MazeSettings GetMazeSettings()
  {
    return new MazeSettings(
      "Assets/Images/Maze-maps/Default/maze-paths.png",
      "Assets/Images/Maze-maps/Default/maze-ghost-zones.png",
      "Assets/Images/Maze-maps/Default/maze-pellets.png",
      "Assets/Images/Maze-maps/Default/maze-ghost-house-wall-tiles.png"
    );
  }

  public static Vector2Int[] GetEnergizerPositions()
  {
    return new Vector2Int[4] {
      new Vector2Int(3, 29),
      new Vector2Int(28, 29),
      new Vector2Int(3, 9),
      new Vector2Int(28, 9)
    };
  }

  /*
   * NOTE:  for now using max value for last chase mode interval
   *        technically incorrect, cause this can lead to a crash in the end
   *        when we do not wrap element index back to the beginning.
   */
  public static GhostModeInterval[] GetGhostModeIntervals()
  {
    /*
     * NOTE:  for now simply return current ghost mode interval list
     *        can be easily altered to use multiple mode intervals
     */
    return new GhostModeInterval[8] {
      new GhostModeInterval(GhostMode.SCATTER, 2),
      new GhostModeInterval(GhostMode.CHASE, 20),
      new GhostModeInterval(GhostMode.SCATTER, 7),
      new GhostModeInterval(GhostMode.CHASE, 20),
      new GhostModeInterval(GhostMode.SCATTER, 5),
      new GhostModeInterval(GhostMode.CHASE, 20),
      new GhostModeInterval(GhostMode.SCATTER, 5),
      new GhostModeInterval(GhostMode.CHASE, int.MaxValue)
    };
  }

  /*
   * NOTE: for now - creating the ghost's settings here
   * future wannahave: use (json file for) different sets of configurations
   */
  public static GhostSettings[] GetGhostSettings() {
    float speed = overallSpeed * 0.75f;

    // different speed settings - norm, fright, tunnel
    float[] speedTypes = new float[3] {
      speed * 0.75f,
      speed * 0.5f,
      speed * 0.4f,
    };

    return new GhostSettings[4] {
      // blinky
      new GhostSettings (
        // position and direction
        new Vector2Int(27, 31),         // start tile
        Maze.Dir.Left,                  // start direction
        // speed - normSpeed, frightSpeed, tunnelSpeed
        speedTypes[0], speedTypes[1], speedTypes[2],
        // path finding fields
        Ghost.CHASEScheme.TargetPacman, // chase scheme
        new Vector2Int(27, 35),         // scatter tile
        // info
        Color.red,
        "blinky"
      ),
      // Inky
      new GhostSettings (
        // position and direction
        new Vector2Int(27, 3),          // start tile
        Maze.Dir.Left,                  // start direction
        // speed - normSpeed, frightSpeed, tunnelSpeed
        speedTypes[0], speedTypes[1], speedTypes[2],
        // path finding fields
        Ghost.CHASEScheme.TargetPacman, // chase scheme
        //Ghost.CHASEScheme.Collaborate, // chase scheme
        new Vector2Int(27, 0),         // scatter tile
        // info
        Color.cyan,
        "inky"
      ),
      // pinky
      new GhostSettings (
        // position and direction
        new Vector2Int(4, 31),          // start tile
        Maze.Dir.Right,                  // start direction
        // speed - normSpeed, frightSpeed, tunnelSpeed
        speedTypes[0], speedTypes[1], speedTypes[2],
        // path finding fields
        Ghost.CHASEScheme.TargetPacman, // chase scheme
        //Ghost.CHASEScheme.AheadOfPacman, // chase scheme
        new Vector2Int(4, 35),         // scatter tile
        // info
        Color.magenta,
        "pinky"
      ),
      // Clyde
      new GhostSettings (
        // position and direction
        new Vector2Int(4, 3),          // start tile
        Maze.Dir.Right,                // start direction
        // speed - normSpeed, frightSpeed, tunnelSpeed
        speedTypes[0], speedTypes[1], speedTypes[2],
        // path finding fields
        Ghost.CHASEScheme.TargetPacman, // chase scheme
        //Ghost.CHASEScheme.CircleAround, // chase scheme
        new Vector2Int(4, 0),         // scatter tile
        // info
        new Color(1f, 0.5f, 0f),
        "clyde"
      ),
    }; // end of new GhostSettings
  } // end GetGhostSettings()
} // end GameSettings class definition


} // end namespace
