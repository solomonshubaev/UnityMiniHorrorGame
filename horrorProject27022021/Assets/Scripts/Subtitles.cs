using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subtitles
{
    private Timer disapearTime; // after how much second subtitle will disapeared
    private string text; //subtitle's text

    public Subtitles(Timer disapearTime, string text)
    {
        this.disapearTime = disapearTime;
        this.text = text;
    }

    public void showSubtitles()
    {
    }

    public Timer DisapearTime { get => disapearTime; set => disapearTime = value; }
    public string Text { get => text; set => text = value; }
}
