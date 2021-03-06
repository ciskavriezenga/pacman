using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace PM {
public class Score : MonoBehaviour
{
  public TMP_Text textScore;
  private int highscore = 0;

  public void Awake()
  {
    textScore = GameObject.Find("TextScore").GetComponent<TMP_Text>();
    textScore.text = 0.ToString();
  }

  public void Add(int points)
  {
      highscore += points;
      textScore.text = highscore.ToString();      
  }

}
}
