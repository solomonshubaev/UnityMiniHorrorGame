using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManagerTutorial : MonoBehaviour
{
    public void Restart()
    {
        // load game scene that we play now
        Application.LoadLevel(Application.loadedLevel);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
