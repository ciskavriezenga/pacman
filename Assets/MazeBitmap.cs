using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeBitmap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
      spriteRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
