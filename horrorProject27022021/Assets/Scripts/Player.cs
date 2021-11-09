using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private FirstPersonController fpsController;
    [SerializeField] private GameObject winPanelTutorial;

    private bool activeFlashLight = false;
    private int health = 100;
    [SerializeField] private GameObject bloodSprites;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    private Timer healthTimer;
    [SerializeField] private float timeToRecoverHP = 10f;
    [SerializeField] private float distanceToInteract = 2.0f;
    private int collectedGold = 0;
    [SerializeField] private int totalGold = 7;
    private AudioManager audioManager;
    private AudioSource mainAudioSource;
    [SerializeField] private AudioSource atmospherAudioSource;
    [SerializeField] private Text goldUiText;
    [SerializeField] private GameObject playerFlashLight;
    [SerializeField] private TempGameManager gameManager;
    [SerializeField] private SubtitleManager subtitleManager;
    public int Health { get => health; set => health = value; }
    public int CollectedGold { get => collectedGold; set => collectedGold = value; }
    public AudioManager AudioManager { get => audioManager; set => audioManager = value; }
    public GameObject PlayerFlashLight { get => playerFlashLight; set => playerFlashLight = value; }
    public bool ActiveFlashLight { get => activeFlashLight; set => activeFlashLight = value; }

    // Start is called before the first frame update
    void Start()
    {
        
        if (Time.timeScale != 1.0f)
            Time.timeScale = 1.0f;
        this.mainAudioSource = GetComponent<AudioSource>();
        this.audioManager = new AudioManager();
        this.healthTimer = new Timer(this.timeToRecoverHP);// set recovering time
        this.atmospherAudioSource.PlayOneShot(this.audioManager.AtmospherMusic[0]);//play first atmospher
    }

    // Update is called once per frame
    void Update()
    {
        this.winByCollectingAllGolds(); // check if player won
        this.healthSystem(); // responsible to health
        this.interactSystem(); // responsible to interations with objects
    }

    private void OnTriggerEnter(Collider collision)
    {   
        if (collision.gameObject.tag == "DarkZone")
        {         
            this.darkZoneAccess(collision);
        }
    }


    //check if player has flashlight to enter dark
    public void darkZoneAccess(Collider collision)
    {
        print("DARK ZONE");
        if (!this.ActiveFlashLight) // if player hasn't flashlight
        {
            this.subtitleManager.showText("This room is too dark");
            // this.subtitle = new Subtitles(new Timer(5f),
            //    "Too dark there");
            //this.subtitleUiText.text = subtitle.Text;
            //this.subtitleActive = true;
            //this.subtitleUi.SetActive(true);
        }
        else
        {
            print("You can enter dark zone");
            Destroy(collision.gameObject);// enable player to go to dark room
        }
    }


    //check if win
    private void winByCollectingAllGolds()
    {
        if(this.collectedGold == this.totalGold)//win
        {

            ////Tutorial
            //this.fpsController.enabled = false; // turn off the FPS_CONTROLLER_SCRIPT
            //Cursor.visible = true; // show cursor
            //Cursor.lockState = CursorLockMode.None; // free cursor
            //this.winPanelTutorial.SetActive(true);//show win screen
            //Time.timeScale = 0.0f; //pause (*)

            this.showScreenAndPause(winScreen);
        }
    }

    // responsible to interations with objects
    private void interactSystem()
    {
        if (Input.GetKeyDown(KeyCode.E)) // if E pressed
        {
            print("E pressed");
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, this.distanceToInteract))
            {
                Transform hitTransform = hit.transform; // get object's transform that hit by ray

                switch (hitTransform.tag)
                {
                    case "Gold":
                        this.hitGold(hitTransform.gameObject); // ray hit gold
                        break;
                    case "FlashLight":
                        this.pickupFlashLight(hit.collider.gameObject);// pickup flashLight
                        break;
                }
                
            }
        }
    }



    private void pickupFlashLight(GameObject flashlightGameobject)
    {
        this.playerFlashLight.SetActive(true);// active player's flashlight
        //this.mainAudioSource.Play()
        this.activeFlashLight = true; // set active flash light to TRUE
        Destroy(flashlightGameobject);
    }
    //updates the UI represeting the gold
    private void updateGoldUI()
    {
        this.goldUiText.text = string.Format("Gold: {0} / {1}", this.collectedGold, totalGold);
    }

    //hit gold function.
    private void hitGold(GameObject goldGameObject)
    {
        this.collectedGold++;
        this.updateGoldUI();
        this.mainAudioSource.PlayOneShot(this.audioManager.SoundClipsDictionary["pickup1"]);// playsound of gold collection
        Destroy(goldGameObject);// destroy gold
    }

    private void showScreenAndPause(GameObject screenToShow)
    {
        this.pauseFpsAndShowCursor(true);
        screenToShow.SetActive(true);
        Time.timeScale = 0.0f;//pause game
    }

    //responsible to health update
    private void healthSystem()
    {
        if(health == 100)
        {
            if (this.bloodSprites.active)//blood is active
                this.bloodSprites.SetActive(false); //hide blood
        }
        else if(health == 50)
        {
            if (!this.bloodSprites.active)//blood doesnt active
                this.bloodSprites.SetActive(true); //show blood
            this.recoverHealth(); // start recover
        }
        else if(this.health <= 0)
        {
            //GameOver
            this.showScreenAndPause(this.loseScreen);
            //animation of death
        }
    }

    //recovering health
    private void recoverHealth()
    {
        if (this.healthTimer.runTimer())//run & check if timer finished
        {
            this.health = 100; // set hp to 100 (healed)
            this.updateUIBlood(false);
            this.healthTimer.resetTimer(this.timeToRecoverHP); //reset timer
        }
    }

    //activating the blood ui
    private void updateUIBlood(bool activeBlood)
    {    
        if(!this.bloodSprites.active)//blood doesnt active
            this.bloodSprites.SetActive(activeBlood); // if hp is 50 show blood    
        else
            this.bloodSprites.SetActive(!activeBlood); // if hp is 50 show blood
    }

    private void pauseFpsAndShowCursor(bool pause)
    {
        this.fpsController.enabled = !pause; // pause game-> unenable FPS
        Cursor.visible = pause;// pause game -> show cursor
        if(pause) // pause -> dont lock cursor
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked; // unpause -> lock cursor

    }
}
