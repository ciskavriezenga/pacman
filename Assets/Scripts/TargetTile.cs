using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetTile : MonoBehaviour
{
  public enum TargetTileType {
    BlinkyRed,
    PinkyPink,
    InkyCyan,
    ClydeOrange
  }

  public TargetTileType targetTiletype;

  #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
      string customName = "targetTiles\\" + targetTiletype.ToString() + ".png";
      Gizmos.DrawIcon(transform.position, customName, true);
    }
  #endif

}
