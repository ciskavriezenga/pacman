using System.Collections.Generic;
using UnityEngine;


namespace PM {


// =============================================================================
// =============== static GameSettings class ===================================
// =============================================================================


// TODO - create an array with arrays for scatter / chase mode per (multiple)
//        levels according to pdf pacman dosier
public static class GameSettings {

  public enum MapType {
    DEFAULT,
    TELEPORTS,
    ENERGIZERS
  }


  private static float overallSpeed = 0.1875f;

  public static PacmanSettings GetPacmanSettings()
  {
    return new PacmanSettings(
    // speed: overall speed, percentages: norm, normDot, fright, frightDot
    overallSpeed, 0.8f, 0.71f, 0.9f, 0.79f,
    // start position and direction
    new Vector2(16f, 9.5f), Dir.LEFT,
    // info - name
    "default-pacman-settings");
  }

  // NOTE: for now passing the map type as a string that corresponds to the
  // folder
  public static MazeSettings GetMazeSettings(MapType mapType)
  {
    string folder;
    switch(mapType) {
      case MapType.TELEPORTS:
        folder = "Teleports";
        break;
      case MapType.ENERGIZERS:
        folder = "Energizers";
        break;
      default:
        folder = "Default";
        break;
    }

    Debug.Log("GameSettings.GetMazeSettings - loading map: Assets/Images/Maze-maps/"+ folder + "/maze-paths.png");
    return new MazeSettings(
      "Assets/Images/Maze-maps/"+ folder + "/maze-paths.png",
      "Assets/Images/Maze-maps/"+ folder + "/maze-ghost-zones.png",
      "Assets/Images/Maze-maps/"+ folder + "/maze-pellets.png",
      "Assets/Images/Maze-maps/"+ folder + "/maze-ghost-house-wall-tiles.png"
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
      new GhostModeInterval(GhostMode.SCATTER, 7),
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
        new Vector2(27f, 31.5f),         // start pos
        Dir.LEFT,                  // start direction
        false,                    // start In Ghosthouse
        // speed - normSpeed, frightSpeed, tunnelSpeed
        speedTypes[0], speedTypes[1], speedTypes[2],
        // path finding fields
        Ghost.ChaseScheme.TARGET_PACMAN, // chase scheme
        new Vector2Int(27, 35),         // scatter tile
        // info
        Color.red,
        "blinky"
      ),
      // Inky
      new GhostSettings (
        // position and direction
        new Vector2(13.875f, 18.5f),          // start pos
        Dir.UP,                  // start direction
        true,                    // start In Ghosthouse
        // speed - normSpeed, frightSpeed, tunnelSpeed
        speedTypes[0], speedTypes[1], speedTypes[2],
        // path finding fields
        Ghost.ChaseScheme.TARGET_PACMAN, // chase scheme
        //Ghost.ChaseScheme.COLLABORATE, // chase scheme
        new Vector2Int(27, 0),         // scatter tile
        // info
        Color.cyan,
        "inky"
      ),
      // pinky
      new GhostSettings (
        // position and direction
        new Vector2(4f, 31.5f),          // start pos
        Dir.RIGHT,                  // start direction
        false,                    // start In Ghosthouse
        // speed - normSpeed, frightSpeed, tunnelSpeed
        speedTypes[0], speedTypes[1], speedTypes[2],
        // path finding fields
        Ghost.ChaseScheme.TARGET_PACMAN, // chase scheme
        //Ghost.ChaseScheme.AHEAD_OF_PACMAN, // chase scheme
        new Vector2Int(4, 35),         // scatter tile
        // info
        Color.magenta,
        "pinky"
      ),
      // Clyde
      new GhostSettings (
        // position and direction
        new Vector2(4f, 3.5f),          // start pos
        Dir.RIGHT,                // start direction
        false,                    // start In Ghosthouse
        // speed - normSpeed, frightSpeed, tunnelSpeed
        speedTypes[0], speedTypes[1], speedTypes[2],
        // path finding fields
        Ghost.ChaseScheme.TARGET_PACMAN, // chase scheme
        //Ghost.ChaseScheme.CIRCLE_AROUND, // chase scheme
        new Vector2Int(4, 0),         // scatter tile
        // info
        new Color(1f, 0.5f, 0f),
        "clyde"
      ),
    }; // end of new GhostSettings
  } // end GetGhostSettings()
} // end GameSettings class definition


} // end namespace
