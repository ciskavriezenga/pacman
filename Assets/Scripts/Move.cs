using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM {

// =============================================================================
// =============== GhostMove ===================================================
// =============================================================================
public struct GhostMove {
  public Vector2Int tile { get; private set; }
  public Dir dir { get; private set; }
  public Vector2Int pixelPos { get; private set; }
  public Vector2 pos { get; private set; }

  public GhostMove(Vector2Int targetTile, Dir dir,
    Vector2Int targetPixelPos, Vector2 targetPos) {
    this.tile = targetTile;
    this.dir = dir;
    this.pixelPos = targetPixelPos;
    this.pos = targetPos;
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
  public Dir startDirection;
  // info
  public string settingsName;

  public PacmanSettings(
    float overallSpeed,
    float normSpeedPerc, float normDotSpeedPerc,
    float frightSpeedPerc, float frightDotSpeedPerc,
    Vector2 startPos, Dir startDirection,
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
// =============== GhostSettings================================================
// =============================================================================
public struct GhostSettings {
  // start fields: position, direction, start in ghostHouse
  public Vector2 startPos;
  public Dir startDirection;
  public bool startInGhosthouse;
  // speed
  public float normSpeed;
  public float frightSpeed;
  public float tunnelSpeed;
  // path finding fields
  public Ghost.ChaseScheme chaseScheme;
  public Vector2Int scatterTile;
  // info
  public Color color;
  public string name;

  public GhostSettings(
    // position and direction
    Vector2 startPos, Dir startDirection, bool startInGhosthouse,
    // speed
    float normSpeed, float frightSpeed, float tunnelSpeed,
    // path finding fields
    Ghost.ChaseScheme chaseScheme, Vector2Int scatterTile,
    // info
    Color color, string name)
  {
    // position and direction
    this.startPos = startPos;
    this.startDirection = startDirection;
    this.startInGhosthouse = startInGhosthouse;
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


}
