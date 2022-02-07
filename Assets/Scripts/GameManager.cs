using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GhostMode
{
  Chase = 0,
  Scatter = 1,
  Frightened = 2,
  NumModes = 3
}

public class GameManager : MonoBehaviour
{
  public int chaseDuration = 10;
  public int scatterDuration = 30;
  public int frightenedDuration = 30;
  public float countdownTime { get; private set; }
  public GhostMode currentGhostMode { get; private set; }

  public Ghost[] ghosts;

  void Awake() {
    countdownTime = (float)chaseDuration;
    currentGhostMode = GhostMode.Chase;
  }
  // Update is called once per frame
  void Update()
  {
    countdownTime-= Time.deltaTime;
    if(countdownTime <= 0) {
      // TODO - fix according to original pacman
      // increment ghost mode
      int tempGhostMode = (int)currentGhostMode + 1;
      // if the num modes are exceded - wrap
      if(tempGhostMode >= (int)GhostMode.NumModes) {
        tempGhostMode -= (int)GhostMode.NumModes;
      }
      currentGhostMode = (GhostMode) tempGhostMode;

      // reset the timer according to the new updated ghostMode
      ResetCountdown();

    }
  }

  void ResetCountdown() {
    switch(currentGhostMode) {
      case GhostMode.Chase:
        countdownTime = (float)chaseDuration;
        break;
      case GhostMode.Scatter:
        countdownTime = (float)scatterDuration;
        break;
      case GhostMode.Frightened:
        countdownTime = (float)frightenedDuration;
        break;
      default:
        throw new System.Exception("GameManager.ResetCountdown " +
          "- invalid ghostMode.");
    }
    Debug.Log("GameManager.ResetCountDown - ghosts.Length" + ghosts.Length);
    // update the ghostmode for each ghost
    for(int i = 0; i < ghosts.Length; i++) {
      ghosts[i].SwitchMode(currentGhostMode);
    }
  }



}
