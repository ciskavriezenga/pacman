using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Test settings")]

public class TestSettings : ScriptableObject
{
  public string testName;
  public int testNumber;
  private string testNamePriv;
  private int testNumberPriv;

  public void Initialize(string name, int number)
  {
    testNamePriv = name;
    testNumberPriv = number;
    Log("INITIALIZE");
  }

  void Awake() {
    Log("AWAKE");
  }

  void OnDestroy() {
    Log("ON-DESTROY");
  }

  void OnDisable() {
    Log("ON-DISABLE");
  }

  void OnEnable() {
    Log("ON-ENABLE");
  }

  void OnValidate() {
    Log("ON-VALIDATE");
  }

  void Reset() {
    Log("RESET");
  }


  public void Log(string fromMethod) {
    Debug.Log("TestSettings - " + fromMethod + " - testName: " + testName);
    Debug.Log("TestSettings - " + fromMethod + " - testNumber: " + testNumber);
    Debug.Log("TestSettings - " + fromMethod + " - testNamePriv: " + testNamePriv);
    Debug.Log("TestSettings - " + fromMethod + " - testNumberPriv: " + testNumberPriv);
  }

}
