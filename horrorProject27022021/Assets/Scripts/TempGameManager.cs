using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempGameManager : MonoBehaviour
{
    private Player player;
    [SerializeField] private AudioSource zombieSpawnAudioSource;
    [SerializeField] private GameObject zombieGameobject;
    [SerializeField] private GameObject subtitleUi;
    private Text subtitleUiText;
    private Subtitles subtitle;
    private bool subtitleActive = false;
    // Start is called before the first frame update
    void Start()
    {
        this.player = this.GetComponent<Player>();
        this.subtitleUiText = this.subtitleUi.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        this.showMonster(); // show monster after collecting 3 gold
        if(this.subtitleActive && !this.subtitleUi.activeSelf) // we need to play and subtitle isnt playing
        {
            if(this.subtitle.DisapearTime.runTimer())//start timer if ended -> true, hide subtitle
            {
                this.subtitleActive = false;
                this.subtitleUi.SetActive(false);
            }
        }
    }



    // active moster
    private void showMonster()
    {
        if(this.player.CollectedGold > 2 && !this.zombieGameobject.activeSelf)
        {
            print("Summon zombie");
            this.zombieSpawnAudioSource.PlayOneShot(this.player.AudioManager.SoundClipsDictionary["explosion1"]);//play Explosion sound
            this.zombieGameobject.SetActive(true);

        }
    }

    


}
