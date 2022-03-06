//#define NO_GHOSTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM {
public enum GhostMode
{
  CHASE = 0,
  SCATTER = 1,
  FRIGHTENED = 2,
  PACING_HOME = 3,
  LEAVING_HOUSE = 4
}

public class GameManager : MonoBehaviour
{

  public static GameManager Instance { get; private set; }
  // reference to the Maze, Pacman and Ghost objects
  [SerializeField] private Maze maze;
  [SerializeField] private Score score;
  [SerializeField] private Pellet[,] pellets;
  [SerializeField] private Pacman pacman;
  // TODO - make private?
  public Ghost[] ghosts {get; private set;}

  // ----------- ghost mode fields -----------
  // array with ghost intervals, containing countdown time and ghost mode
  private GhostModeInterval[] ghostModeIntervals;
  // current ghost mode interval index
  private int gModeIntervalIndex;
  // current ghost mode
  [SerializeField] public GhostMode currentGhostMode { get; private set; }

  // countdown time of the scatter chase timer
  [SerializeField] public float scatterChaseTime { get; private set; }
  // countdown time of the scatter chase timer
  [SerializeField] public float frightenedTime { get; private set; }

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
    // initialize the random number generator with a seed
    Random.InitState(0);

    // create and instantiate the Maze GameObject
    maze = GameFactory.InstantiatePrefab("Prefabs/Maze", "maze").GetComponent<Maze>();
    maze.Initialize(GameSettings.GetMazeSettings());

    // create and instantiate the highscore GameObject
    score = GameFactory.InstantiatePrefab("Prefabs/Score", "score").GetComponent<Score>();
    // create and instantiate pellets
    pellets = GameFactory.InstantiatePellets("Prefabs/Pellet", maze, score,
      GameSettings.GetEnergizerPositions());

    // create and instantiate the Pacman GameObject
    pacman = GameFactory.InstantiatePrefab("Prefabs/Pacman", "pacman").GetComponent<Pacman>();
    pacman.Initialize(GameSettings.GetPacmanSettings(), this);

#if !NO_GHOSTS
    // create the ghosts
    ghosts = GameFactory.InstantiateGhosts(GameSettings.GetGhostSettings(), this);
#endif

    // retrieve the ghostModeIntervals to manage countdown times and ghostmodes
    ghostModeIntervals = GameSettings.GetGhostModeIntervals();
  }



  // initialize values on Awake
  private void Start() {
    Debug.Log("GameManager - START");
    // start with first ghost mode interval
    ResetGhostModeInterval();
  }

  // Update is called once per frame
  private void Update()
  {
    if(currentGhostMode == GhostMode.FRIGHTENED){
      frightenedTime -= Time.deltaTime;
      if(frightenedTime <= 0) {
        UpdateGhostMode(ghostModeIntervals[gModeIntervalIndex].mode);
      }
    } else {
      // scatter chase timer is running
      scatterChaseTime-= Time.deltaTime;
      if(scatterChaseTime <= 0) {
        NextGhostModeInterval();
      }
    }

  }



// =============================================================================
// =============== ghost mode methods  =========================================
// =============================================================================

  public void EnergizerIsEaten()
  {
    Debug.Log( " Energizer is EATEN! ");
    UpdateGhostMode(GhostMode.FRIGHTENED);
    frightenedTime = 4f;
  }

  private void NextGhostModeInterval()
  {
    // increment ghost mode interval index
    gModeIntervalIndex++;
    // update ghosts  current index
    UpdateGhostModeInterval();
  }

  private void ResetGhostModeInterval()
  {
    // reset ghost mode interval index to first interval
    gModeIntervalIndex = 0;
    // update ghosts  current index
    UpdateGhostModeInterval();
  }

  private void UpdateGhostModeInterval()
  {
    // reset the timer to the interval of the current ghost mode
    scatterChaseTime = ghostModeIntervals[gModeIntervalIndex].interval;
    UpdateGhostMode(ghostModeIntervals[gModeIntervalIndex].mode);
  }

  private void UpdateGhostMode(GhostMode mode) {
    // cache the current ghost mode
    currentGhostMode = mode;
    // update the ghostmode for each ghost
#if !NO_GHOSTS
    Debug.Log("GameManager-UpdateGhostMode - New ghostmode: " + currentGhostMode);
    for(int i = 0; i < ghosts.Length; i++) {
      ghosts[i].SwitchMode(currentGhostMode);
    }
#endif
  }

// =============================================================================
// =============== Pellets  ====================================================
// =============================================================================

  public bool PacmanEatsPellet(Vector2Int tile) {
    if(pellets[tile.x, tile.y] != null) {
      pellets[tile.x, tile.y].GetsEaten();
      return true;
    }
    return false;
  }


// =============================================================================
// =============== getters  ====================================================
// =============================================================================

  public bool PathIsEmpty(Vector2Int tile) {
    for(int i = 0; i < ghosts.Length; i++) {
      if(ghosts[i].GetCurrentTile() == tile) return false;
    }
    if(pacman.GetCurrentTile() == tile) return false;
    return true;
  }


  public bool GameModeIsFrightened ()
  {
    return currentGhostMode == GhostMode.FRIGHTENED;
  }
  public Maze GetMaze() {
    return maze;
  }
  public Pacman GetPacman() { return pacman; }
}
}
