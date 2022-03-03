using System.Collections.Generic;
using UnityEngine;


namespace PM {


// =============================================================================
// =============== MazeSettings ================================================
// =============================================================================
  public struct MazeSettings {
    public int width;
    public int height;
    public string imgPathMultiBg;
    public string imgPathGhostHouse;

    public MazeSettings(string imgPathMultiBg, string imgPathGhostHouse)
    {
      width = 32;
      height = 35;
      this.imgPathMultiBg = imgPathMultiBg;
      this.imgPathGhostHouse = imgPathGhostHouse;
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
    public Vector2Int startTile;
    public Vector2Int scatterTile;
    public Maze.Dir startDirection;
    public float normalSpeed;
    public Ghost.CHASEScheme chaseScheme;
    // TODO - replace with animation
    public Color color;
    public string name;

    public GhostSettings(Vector2Int startTile, Vector2Int scatterTile,
      Maze.Dir startDirection, float normalSpeed, Ghost.CHASEScheme chaseScheme,
      Color color, string name)
    {
      this.startTile = startTile;
      this.scatterTile = scatterTile;
      this.startDirection = startDirection;
      this.normalSpeed = normalSpeed;
      this.chaseScheme = chaseScheme;
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
        "Assets/Images/Maze-maps/Default/pacman-bg-multi.png",
        "Assets/Images/Maze-maps/Default/pacman-bg-ghost-house.png");
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
      return new GhostSettings[4] {
      // blinky
      new GhostSettings (
        new Vector2Int(27, 31),   // start tile
        new Vector2Int(27, 35),   // scatter tile
        Maze.Dir.Left,            // start direction
        speed,                      // normal speed
        Ghost.CHASEScheme.TargetPacman, // chase scheme
        Color.red,
        "blinky"
      ),
      // Inky
      new GhostSettings (
        new Vector2Int(27, 3),   // start tile
        new Vector2Int(27, 0),   // scatter tile
        Maze.Dir.Left,           // start direction
        speed,                     // normal speed
        Ghost.CHASEScheme.TargetPacman, // chase scheme
        //Ghost.CHASEScheme.Collaborate, // chase scheme
        Color.cyan,
        "inky"
      ),
      // pinky
      new GhostSettings (
        new Vector2Int(4, 31),    // start tile
        new Vector2Int(4, 35),    // scatter tile
        Maze.Dir.Right,           // start direction
        speed,                      // normal speed
        Ghost.CHASEScheme.TargetPacman, // chase scheme
        //Ghost.CHASEScheme.AheadOfPacman,// chase scheme
        Color.magenta,
        "pinky"
      ),
      // Clyde
      new GhostSettings (
        new Vector2Int(4, 3),    // start tile
        new Vector2Int(4, 0),   // scatter tile
        Maze.Dir.Left,           // start direction
        speed,                     // normal speed
        Ghost.CHASEScheme.TargetPacman, // chase scheme
        //Ghost.CHASEScheme.CircleAround, // chase scheme
        new Color(1f, 0.5f, 0f),
        "clyde"
      )};
  }

  }
}
