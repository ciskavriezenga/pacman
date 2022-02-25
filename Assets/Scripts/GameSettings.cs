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
    public Vector2 startPos;
    public float speed;
    public Maze.Dir startDirection;
    public string settingsName;
    public PacmanSettings(Vector2 startPos, float speed,
      Maze.Dir startDirection, string settingsName)
    {
      this.startPos = startPos;
      this.speed = speed;
      this.startDirection = startDirection;
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
    public Ghost.ChaseScheme chaseScheme;
    // TODO - replace with animation
    public Color color;
    public string name;

    public GhostSettings(Vector2Int startTile, Vector2Int scatterTile,
      Maze.Dir startDirection, float normalSpeed, Ghost.ChaseScheme chaseScheme,
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
    public static PacmanSettings GetPacmanSettings()
    {
      return new PacmanSettings(new Vector2(16f, 9.5f), 0.1f, Maze.Dir.Left,
      "default-pacman-settings");
    }

    public static MazeSettings GetMazeSettings() {
      return new MazeSettings(
        "Assets/Images/Maze-maps/Default/pacman-bg-multi.png",
        "Assets/Images/Maze-maps/Default/pacman-bg-ghost-house.png");
    }

    /*
     * NOTE:  for now using max value for last chase mode interval
     *        technically incorrect, cause this can lead to a crash in the end
     *        when we do not wrap element index back to the beginning.
     */
    public static readonly GhostModeInterval[] ghostModeIntervals = {
      new GhostModeInterval(GhostMode.Scatter, 2),
      new GhostModeInterval(GhostMode.Chase, 20),
      new GhostModeInterval(GhostMode.Scatter, 7),
      new GhostModeInterval(GhostMode.Chase, 20),
      new GhostModeInterval(GhostMode.Scatter, 5),
      new GhostModeInterval(GhostMode.Chase, 20),
      new GhostModeInterval(GhostMode.Scatter, 5),
      new GhostModeInterval(GhostMode.Chase, int.MaxValue)
    };

    /*
     * NOTE: for now - creating the ghost's settings here hardocded
     * future wannahave: use json file for different sets of configurations
     */
    public static GhostSettings[] GetGhostSettings() {

      return new GhostSettings[4] {
      // blinky
      new GhostSettings (
        new Vector2Int(27, 31),   // start tile
        new Vector2Int(27, 35),   // scatter tile
        Maze.Dir.Left,            // start direction
        0.1f,                      // normal speed
        Ghost.ChaseScheme.TargetPacman, // chase scheme
        Color.red,
        "blinky"
      ),
      // Inky
      new GhostSettings (
        new Vector2Int(27, 3),   // start tile
        new Vector2Int(27, 0),   // scatter tile
        Maze.Dir.Left,           // start direction
        0.1f,                     // normal speed
        Ghost.ChaseScheme.TargetPacman, // chase scheme
        //Ghost.ChaseScheme.Collaborate, // chase scheme
        Color.cyan,
        "inky"
      ),
      // pinky
      new GhostSettings (
        new Vector2Int(4, 31),    // start tile
        new Vector2Int(4, 35),    // scatter tile
        Maze.Dir.Right,           // start direction
        0.1f,                      // normal speed
        Ghost.ChaseScheme.TargetPacman, // chase scheme
        //Ghost.ChaseScheme.AheadOfPacman,// chase scheme
        Color.magenta,
        "pinky"
      ),
      // Clyde
      new GhostSettings (
        new Vector2Int(4, 3),    // start tile
        new Vector2Int(4, 0),   // scatter tile
        Maze.Dir.Left,           // start direction
        0.1f,                     // normal speed
        Ghost.ChaseScheme.TargetPacman, // chase scheme
        //Ghost.ChaseScheme.CircleAround, // chase scheme
        new Color(1f, 0.5f, 0f),
        "clyde"
      )};
  }

  }
}
