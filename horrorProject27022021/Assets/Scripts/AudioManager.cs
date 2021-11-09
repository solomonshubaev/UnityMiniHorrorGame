using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    private Dictionary<string, AudioClip> soundClipsDictionary;
    private List<AudioClip> atmospherMusic;// music

    public List<AudioClip> AtmospherMusic { get => atmospherMusic; set => atmospherMusic = value; }
    public Dictionary<string, AudioClip> SoundClipsDictionary { get => soundClipsDictionary; set => soundClipsDictionary = value; }

    public AudioManager()
    {
        this.initAudioManager();
    }

    public void initAudioManager()
    {
        this.soundClipsDictionary = new Dictionary<string, AudioClip>();
        this.atmospherMusic = new List<AudioClip>();

        //music atmospher
        this.atmospherMusic.Add(Resources.Load<AudioClip>("Atmos 2"));

        //sounds
        this.soundClipsDictionary.Add("pickup1", Resources.Load<AudioClip>("pickupCoin0"));
        this.soundClipsDictionary.Add("explosion1", Resources.Load<AudioClip>("Explosion 1"));
        this.soundClipsDictionary.Add("flashLight1", Resources.Load<AudioClip>("Explosion 1"));

        
    }
}
