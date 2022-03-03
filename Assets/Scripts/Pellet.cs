using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PelletType {
  PELLET = 0,
  ENERGIZER = 1
}

namespace PM {
public class Pellet : MonoBehaviour
{
  private bool isEnergizer;
  private Score score;

  public void Initialize(bool isEnergizer, Score score)
  {
    this.isEnergizer = isEnergizer;

    this.score = score;

    string spritePath = isEnergizer ? "SuperPellet" :  "Pellet";
    GetComponent<SpriteRenderer>().sprite =  Resources.Load(spritePath, typeof(Sprite)) as Sprite;
  }

  // OnTriggerEnter2D is called when Pacman triggers a pellet
  public void GetsEaten()
  {
    int points = isEnergizer ? 50 : 10;
    score.Add(points);

    if(isEnergizer) GameManager.Instance.EnergizerIsEaten();

    Destroy(gameObject);
  }
}
}
