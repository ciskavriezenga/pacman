using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM {
public class Pellet : MonoBehaviour
{
  private bool isSuperPellet;
  private Score score;

  public void Initialize(bool isSuperPellet, Score score)
  {
    this.isSuperPellet = isSuperPellet;
    this.score = score;

    /*if(isSuperPellet) {
      GetComponent<SpriteRenderer>().sprite =  Resources.Load("SuperPellet", typeof(Sprite)) as Sprite;
    } else {
      GetComponent<SpriteRenderer>().sprite =  Resources.Load("Pellet", typeof(Sprite)) as Sprite;
    }*/

  }
  // OnTriggerEnter2D is called when Pacman triggers a pellet
  void OnTriggerEnter2D(Collider2D co)
  {
    Debug.Log("trigger!");
    if (co.name == "pacman")
    {
      Destroy(gameObject);
      // TODO - score based on pellet type
      score.increment();
      // TODO - add super pellet functionality
    }
  }
}
}
