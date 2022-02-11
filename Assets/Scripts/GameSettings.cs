using System.Collections.Generic;
using UnityEngine;


namespace PM {


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
    public Grid.Dir startDirection;
    public float normalSpeed;
    public Ghost.ChaseScheme chaseScheme;
    // TODO - replace with animation
    public Color color;
    public string name;

    public GhostSettings(Vector2Int startTile, Vector2Int scatterTile,
      Grid.Dir startDirection, float normalSpeed, Ghost.ChaseScheme chaseScheme,
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
    // NOTE:  for now using max value for last chase mode interval
    //        technically incorrect, cause this can lead to a crash in the end
    //        when we do not wrap element index back to the beginning.
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
    public static readonly GhostSettings[] ghostSettingsGhosts = {
      // blinky
      new GhostSettings (
        new Vector2Int(27, 31),   // start tile
        new Vector2Int(27, 35),   // scatter tile
        Grid.Dir.Left,            // start direction
        0.1f,                      // normal speed
        Ghost.ChaseScheme.TargetPacman, // chase scheme
        Color.red,
        // TODO generate name instead
        "ghost_1"
      ),
      // Inky
      new GhostSettings (
        new Vector2Int(27, 3),   // start tile
        new Vector2Int(29, 0),   // scatter tile
        Grid.Dir.Left,           // start direction
        0.1f,                     // normal speed
        Ghost.ChaseScheme.TargetPacman, // chase scheme
        //Ghost.ChaseScheme.Collaborate, // chase scheme
        Color.cyan,
        "ghost_2"
      ),
      // pinky
      new GhostSettings (
        new Vector2Int(4, 31),    // start tile
        new Vector2Int(4, 35),    // scatter tile
        Grid.Dir.Right,           // start direction
        0.1f,                      // normal speed
        Ghost.ChaseScheme.TargetPacman, // chase scheme
        //Ghost.ChaseScheme.AheadOfPacman,// chase scheme
        Color.magenta,
        "ghost_3"
      ),
      // Clyde
      new GhostSettings (
        new Vector2Int(4, 3),    // start tile
        new Vector2Int(4, 0),   // scatter tile
        Grid.Dir.Left,           // start direction
        0.1f,                     // normal speed
        Ghost.ChaseScheme.TargetPacman, // chase scheme
        //Ghost.ChaseScheme.CircleAround, // chase scheme
        new Color(1f, 0.5f, 0f),
        "ghost_4"
      )
    };

  }
}
