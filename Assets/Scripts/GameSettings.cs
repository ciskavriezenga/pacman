using System.Collections.Generic;

namespace PM {

  public struct GhostModeInterval {
    public GhostMode mode {get; private set;}
    public int interval {get; private set;}

    public GhostModeInterval(GhostMode mode, int interval)
    {
      this.mode = mode;
      this.interval = interval;
    }
  }
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
  }


}
