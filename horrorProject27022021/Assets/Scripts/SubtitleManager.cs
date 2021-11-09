using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleManager : MonoBehaviour
{
    [SerializeField] private float timeToDisapear = 3.5f;
    private string textToShow;
    private Timer timer;
    private bool enableSubtitle = false; // if subtitle is enable so run timer
    [SerializeField] private Text textUI;

    public bool EnableSubtitle { get => enableSubtitle; set => enableSubtitle = value; }
    public string TextToShow { get => textToShow; set => textToShow = value; }

    void Start()
    {
        this.timer = new Timer(this.timeToDisapear);// create new timer
    }

    // Update is called once per frame
    void Update()
    {
        if(this.enableSubtitle)//if subtitle is ON (enabled)
        {
            if(this.timer.runTimer())//run timer, if time end return true
            {
                print("timer is ended");
                this.textUI.text = Definitions.EMPTY_STRING;
                this.enableSubtitle = false;
            }
        }
    }

    public void showText(string textToShow)
    {
        this.enableSubtitle = true;
        this.timer.resetTimer(this.timeToDisapear); // set time to timer
        this.textToShow = textToShow; //set text to show
        this.textUI.text = this.textToShow;
    }
}


