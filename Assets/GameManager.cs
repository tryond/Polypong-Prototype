using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int targetFrameRate = 60;
    
    public void Start()
    {
        Application.targetFrameRate = targetFrameRate;
    }

    public void Restart()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
    
}
