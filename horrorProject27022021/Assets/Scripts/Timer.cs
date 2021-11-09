using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private float timeRemaining;
    private bool isTimerEnded; //if time  running -> false

    public float TimeRemaining { get => timeRemaining; set => timeRemaining = value; }

    public Timer(float time)
    {
        this.isTimerEnded = true;
        this.timeRemaining = time;
    }

    //play timer, decreasing from remaining time time.
    //if finished -> return True
    public bool runTimer()
    {
        if (this.timeRemaining > 0.0f && !this.isTimerEnded) // timer is not finished
        {
            this.timeRemaining -= Time.deltaTime;
            return false;
        }
         // timer finished
         this.isTimerEnded = true;
         return true;
    }

    public void resetTimer(float newTime)
    {
        this.timeRemaining = newTime;
        this.isTimerEnded = false;
    }
}
