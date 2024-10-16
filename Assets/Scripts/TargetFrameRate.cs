using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFrameRate : MonoBehaviour
{
    public int TargetFPS = 60;
    void Start()
    {
        Application.targetFrameRate = TargetFPS;
    }

}
