using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM {

public static class GhostFactory {

  static public GameObject[] CreateGhosts(GhostSettings[] ghostSettingsGhosts,
    GameManager gameManager)
  {
    // create the ghosts GameObject array
    int numGhosts = ghostSettingsGhosts.Length;
    GameObject[] ghosts = new GameObject[numGhosts];

    // create a ghost for each GameSetting in settings
    for(int i = 0; i < numGhosts; i++) {
      ghosts[i] = CreateGhost(ghostSettingsGhosts[i], gameManager);
      Debug.Log("GhostFactory.CreateGhosts - ghosts[i]: " + ghosts[i]);

      // set wingman for all ghosts except for first - use previous ghost
      if(i > 0) {
        ghosts[i].GetComponent<Ghost>().wingman =
          ghosts[i - 1].GetComponent<Ghost>();
      }
    }
    // set last ghost as wingman of first ghost
    ghosts[0].GetComponent<Ghost>().wingman =
      ghosts[numGhosts - 1].GetComponent<Ghost>();
    return ghosts;
  }


  static public GameObject CreateGhost(GhostSettings settings,
   GameManager gameManager)
  {
    // create an empty game object
    GameObject ghost = new GameObject();
    ghost.name = settings.name;
    // add rigidbody component
    Rigidbody ghostRigidBody = ghost.AddComponent<Rigidbody>() as Rigidbody;
    ghostRigidBody.angularDrag = 0.0f;
    ghostRigidBody.useGravity = false;

    // create ghost sprite renderer texture2D
    // TODO - use animation intead
    AddSpriteRenderer(ghost, "./assets/artwork/ghost.png", settings.color, 3);
    AddGhostScriptComponent(ghost, settings, gameManager);
    AddScatterTileSR(ghost, settings, gameManager);
    AddTargetTileSR(ghost, settings, gameManager);

    return ghost;
  }

  static void AddSpriteRenderer(GameObject gameObject, string imgPath,
    Color color, int sortingOrder)
  {
    // add sprite renderer
    SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>()
        as SpriteRenderer;
    spriteRenderer.sortingOrder = sortingOrder;

    // load image data
    byte[] imgData = System.IO.File.ReadAllBytes(imgPath);
    Texture2D texture = new Texture2D(1, 1);
    texture.LoadImage(imgData);
    spriteRenderer.color = color;
    Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width,
      texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    spriteRenderer.sprite = sprite;
  }

  static void AddGhostScriptComponent(GameObject gameObject,
    GhostSettings settings, GameManager gameManager)
  {
    // add Ghost script component
    gameObject.AddComponent<Ghost>();
    // initialize ghost script component
    gameObject.GetComponent<Ghost>().Initialize(settings, gameManager,
      gameManager.grid,  gameManager.pacmanMov);
  }

  static void AddScatterTileSR(GameObject ghost, GhostSettings settings,
    GameManager gameManager)
  {
    /*
     * NOTE:  duplicate code - AddTargetTileSR
     *        but okay for now, debugging purpose
     */
    // create targetTileVisualiser
    GameObject scatterTileSR = new GameObject();
    scatterTileSR.name = settings.name + "-scatter-tile-SR";
    AddSpriteRenderer(scatterTileSR, "./assets/artwork/targetTile.png",
      settings.color, 4);
    scatterTileSR.transform.position = gameManager.grid.GetCenterPos(settings.scatterTile);
    ghost.GetComponent<Ghost>().scatterTileSR = scatterTileSR;
  }

  static void AddTargetTileSR(GameObject ghost, GhostSettings settings,
    GameManager gameManager)
  {
    // create targetTileVisualiser
    GameObject targetTileSR = new GameObject();
    targetTileSR.name = settings.name + "-target-tile-SR";
    AddSpriteRenderer(targetTileSR, "./assets/artwork/targetTile.png",
      settings.color, 4);
    targetTileSR.transform.position = gameManager.grid.GetCenterPos(settings.scatterTile);
    ghost.GetComponent<Ghost>().targetTileSR = targetTileSR;
  }

} // end GhostFactory class
} // end namespace
