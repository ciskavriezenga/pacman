using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM {
  public enum GhostMode
  {
    Chase = 0,
    Scatter = 1,
    Frightened = 2,
    NumModes = 3
  }

  public class GameManager : MonoBehaviour
  {
    public Ghost[] ghosts;

    public GhostMode currentGhostMode { get; private set; }

    public float countdownTime { get; private set; }
    // current ghost mode interval index
    private int gModeIntervalIndex;

    // initialize values on Awake
    void Start() {
      ResetGhostMode();
    }

    // Update is called once per frame
    void Update()
    {
      countdownTime-= Time.deltaTime;
      Debug.Log("countdownTime: " + countdownTime);
      if(countdownTime <= 0) {
        NextGhostMode();
      }
    }

// =============================================================================
// =============== ghost mode methods  =========================================
// =============================================================================
    void NextGhostMode()
    {
      // increment ghost mode interval index
      gModeIntervalIndex++;
      // update ghosts  current index
      UpdateGhostMode();
    }

    void ResetGhostMode()
    {
      // increment ghost mode interval index
      gModeIntervalIndex = 0;
      // update ghosts  current index
      UpdateGhostMode();
    }

    void UpdateGhostMode() {
      GhostModeInterval modeInterval = GameSettings.ghostModeIntervals[gModeIntervalIndex];

      // reset the timer to the interval of the current ghost mode
      countdownTime = modeInterval.interval;
      // cache the current ghost mode
      currentGhostMode = modeInterval.mode;
      // update the ghostmode for each ghost
      for(int i = 0; i < ghosts.Length; i++) {
        ghosts[i].SwitchMode(currentGhostMode);
      }
    }


  }
}
