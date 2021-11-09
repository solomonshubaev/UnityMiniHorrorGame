using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public void restartGame()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    public void quitGame()
    {
        Application.Quit();
    }


    public void startGame()
    {
        Application.LoadLevel(1);//go To first level
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
