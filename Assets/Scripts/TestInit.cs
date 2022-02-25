using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInit : MonoBehaviour
{
    public string testNamePublic;
    private string testNamePrivate;

    public void Initialize(string name) {
      this.testNamePublic = name;
      this.testNamePrivate = name;
      Log("INITIALIZE");
    }

    void Log(string fromMethod) {
      Debug.Log("TestInit - " + fromMethod + " - testNamePublic: " + testNamePublic);
      Debug.Log("TestInit - " + fromMethod + " - testNamePrivate: " + testNamePrivate);
    }

    /*Start()
    Update()
    OnDisable()
    OnEnable()
    OnValidate()*/


    // Update is called once per frame
    void Awake()
    {
      Log("AWAKE");
    }

    // Start is called before the first frame update
    void Start()
    {
      Log("START");
    }

    // Update is called once per frame
    void Update()
    {
      Log("UPDATE");
    }

    void OnValidate() {
      Log("ON-VALIDATE");
    }

    void Reset() {
      Log("RESET");
    }
}
