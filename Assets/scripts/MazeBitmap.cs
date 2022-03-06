using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TEMP CLASS - to turn of the maze bitmap
namespace PM {
  public class MazeBitmap : MonoBehaviour
  {
      // Start is called before the first frame update
      void Start()
      {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
      }
  }
}
