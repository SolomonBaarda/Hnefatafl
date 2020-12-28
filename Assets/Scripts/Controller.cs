using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public Controller Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }






}
