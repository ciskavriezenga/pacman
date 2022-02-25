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

  public static GameManager Instance { get; private set; }

  // TODO  - make private where possible
  // reference to the Maze, Pacman and Ghost objects
  [SerializeField] private Maze maze;
  [SerializeField] private Pacman pacman;

  [SerializeField] public GhostMode currentGhostMode { get; private set; }
  public GameObject[] ghosts {get; private set;}

  [SerializeField] public float countdownTime { get; private set; }
  // current ghost mode interval index
  private int gModeIntervalIndex;

  public void Awake() {
    // Singleton pattern
    if (Instance != null && Instance != this)
    {
        Destroy(this);
    }
    else
    {
        Instance = this;
    }

    // create and instantiate the Maze GameObject
    maze = GameFactory.InstantiatePrefab("Prefabs/Maze").GetComponent<Maze>();
    maze.Initialize(GameSettings.GetMazeSettings());

    // create and instantiate the Pacman GameObject
    pacman = GameFactory.InstantiatePrefab("Prefabs/Pacman").GetComponent<Pacman>();
    pacman.Initialize(GameSettings.GetPacmanSettings());



    // create the ghosts
#if CREATE_GHOSTS
    ghosts = GhostFactory.InstantiateGhosts(GameSettings.ghostSettingsGhosts, this);
#endif
  }



  // initialize values on Awake
  void Start() {
    Debug.Log("GameManager - START");
    //pacmanMov.Initialize(GameSettings.pacmanSettings, maze);
    ResetGhostMode();
  }

  // Update is called once per frame
  void Update()
  {
    countdownTime-= Time.deltaTime;
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
#if CREATE_GHOSTS
    for(int i = 0; i < ghosts.Length; i++) {
      ghosts[i].GetComponent<Ghost>().SwitchMode(currentGhostMode);
    }
#endif
  }

  public Maze GetMaze() { return maze; }
  public Pacman GetPacman() { return pacman; }
}
}
