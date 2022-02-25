#define SHOW__GHOST_TARGET_TILE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM {

public static class GameFactory {

  public static GameObject InstantiatePrefab(string resourcePath) {
    GameObject prefab = Resources.Load(resourcePath) as GameObject;
    prefab = GameObject.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
    return prefab;
  }


  static public Ghost[] InstantiateGhosts(GhostSettings[] settingsGhosts,
    GameManager gameManager)
  {
    // create the ghosts GameObject array
    int numGhosts = settingsGhosts.Length;
    Ghost[] ghosts = new Ghost[numGhosts];

    // create a ghost for each GameSetting in settings
    for(int i = 0; i < numGhosts; i++) {
      ghosts[i] = InstantiateGhost(settingsGhosts[i], gameManager);

      // set wingman for all ghosts except for first - use previous ghost
      if(i > 0) {
        ghosts[i].wingman = ghosts[i - 1];
      }
    }
    // set last ghost as wingman of first ghost
    ghosts[0].wingman = ghosts[numGhosts - 1];
    return ghosts;
  }


  public static Ghost InstantiateGhost(GhostSettings settings,
    GameManager gameManager)
  {
    // create and instantiate the Ghost GameObject
    Ghost ghost = GameFactory.InstantiatePrefab("Prefabs/Ghost").GetComponent<Ghost>();
    ghost.Initialize(settings, gameManager);
    // TODO - use correct annimation controller

#if SHOW__GHOST_TARGET_TILE
    // add target tile for debugging purposes
    ghost.GetComponent<Ghost>().targetTileSR =
      CreateTargetTileSR(settings, gameManager, "-target-tile-SR");
    // add scatter tile for debugging purposes
    ghost.GetComponent<Ghost>().targetTileSR =
      CreateTargetTileSR(settings, gameManager, "-scatter-tile-SR");
#endif
    return ghost;
  }

  static GameObject CreateTargetTileSR(GhostSettings settings,
    GameManager gameManager, string suffix)
  {
    // create target tile sprite renderer
    GameObject targetTileSR = new GameObject();
    targetTileSR.name = settings.name + suffix;
    AddSpriteRenderer(targetTileSR, "./assets/artwork/targetTile.png",
      settings.color, 4);
    targetTileSR.transform.position = gameManager.GetMaze().GetCenterPos(settings.scatterTile);
    return targetTileSR;
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
}
}
