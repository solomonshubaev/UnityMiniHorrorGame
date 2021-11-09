using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class Agent : MonoBehaviour
{
    [SerializeField] private List<Transform> waypointList;
    [SerializeField] private Transform zombiesHeadPos;
    [SerializeField] private Transform targetTransform; // can be changed to gameobject****
    private Player playerClass;

    [SerializeField] private float maxAngleOfDetection;
    [SerializeField] private float detectionDistance;
    private bool isInChasingMode = false;
    [SerializeField] private Queue<Transform> waypointQueue;
    [SerializeField] private Transform lastTargetPos;
    private NavMeshAgent navAgent;
    private Animator animator;
    [SerializeField] private int currentDestinationIndex = 0;
    private float smoothAngel;
    private float angle;
    private AiSensors aiSensors;
    private Transform lastPlayerTransform; // stores last transform of player
    private Vector3 lastPlayerPosition; // stores last position of player
    [SerializeField] private float distanceToAttack = 2.5f; // distance that AI can attack player
    [SerializeField] private int damagePerHit = 50;
    private float realodeTimeAfterAttack = 3.0f;
    private bool isZombieAttacked = false; // if zombie did attack -> true
    private Timer attackingTimer;
    private AnimatorStateInfo attackAnimator;
    private AudioSource zombieAudioSource;

    [SerializeField] private List<AudioClip> attackAudioList;
    // Start is called before the first frame update
    void Start()
    {
        this.initAudio();
        this.attackingTimer = new Timer(this.realodeTimeAfterAttack); // set timer
        this.playerClass = this.targetTransform.gameObject.GetComponent<Player>(); // get player class
        this.aiSensors = new AiSensors(this.maxAngleOfDetection, this.transform.forward,
            this.zombiesHeadPos, this.detectionDistance); // create sensors
        this.navAgent = this.GetComponent<NavMeshAgent>();
        this.navAgent.updateRotation = false; // we dont want nav agent to update the rotation, locomotion's job
        this.animator = this.GetComponent<Animator>();
        this.waypointQueue = new Queue<Transform>();// init waypoints queue
        foreach (Transform transformValue in this.waypointList)
        {
            this.waypointQueue.Enqueue(transformValue);
        }
        this.setNextDestination(false); // set the agent on his way
        this.attackAnimator = this.animator.GetCurrentAnimatorStateInfo(1); // get attack layer

    }

    // Update is called once per frame
    void Update()
    {
        //this.testAIdetection(); // TEST

        if(this.waypointQueue == null)// check if there is waypoints to agent
        {
            print("No Waypoints to agent");
            return;
        }

        this.calculateVelocityAndAngle();// pass new velocity & angle to locomotion
        this.checkNextDestination();// checks destinations options

        

    }

    // method that responsibile to attack if zombie can
    private void attackIfCan()
    {
        this.attackAnimator = this.animator.GetCurrentAnimatorStateInfo(1);
        
        if (this.isInChasingMode &&
            (Vector3.Distance(this.transform.position, this.targetTransform.position) < this.distanceToAttack))//check if AI can attack player
        {
            if (!this.isAttackAnimationIsPlaying(attackAnimator)// attack animation is not playing -> not attacking)
                && this.attackAnimator.IsName("no attacking")) // and currently state is not attacking
            {
                this.animator.SetTrigger("attack"); // play attack animation

                this.isZombieAttacked = false; // zombie is ready to attack ** Maybe could optimized

                if (!this.zombieAudioSource.isPlaying) // check if there is sound playing
                    this.zombieAudioSource.PlayOneShot(this.attackAudioList[Random.Range(0,this.attackAudioList.Count)]);//play random audio for attacking
                
            }
            else //if attack animation is ON
            {
                this.animator.ResetTrigger("attack");
                this.attackPlayer(attackAnimator); // attack
            }

            //Thread attackThread = new Thread(() => { attackPlayer(currentAnimatorState); }); // thread for attacking
            //attackThread.Start(); //Start THE THREADDDDDDDDDDDD

            //this.attackPlayer(); // attack player
        }
        
    }
    void OnAnimatorMove()
    {
        
        //this.transform.rotation = this.animator.rootRotation; // update rotation from animator
        if(Time.timeScale != 0)
            this.navAgent.velocity = this.animator.deltaPosition / Time.deltaTime; // override nav velocity
    }

    //method that inits all audios
    private void initAudio()
    {
        this.zombieAudioSource = this.GetComponent<AudioSource>();
        this.attackAudioList.Add(Resources.Load<AudioClip>("Zombie003_Attack_A_001"));
        this.attackAudioList.Add(Resources.Load<AudioClip>("Zombie003_Attack_A_002"));
        this.attackAudioList.Add(Resources.Load<AudioClip>("Zombie003_Attack_A_003"));
    }

    //return true if animation is playing
    public bool isAttackAnimationIsPlaying(AnimatorStateInfo currentAnimatorState)
    {
        return currentAnimatorState.IsName("attack");
        //return currentAnimatorState.normalizedTime > 0.0f &&
        //    currentAnimatorState.normalizedTime < currentAnimatorState.length;
    }

    private void attackPlayer(AnimatorStateInfo currentAnimatorState)
    {
        if ((currentAnimatorState.normalizedTime > 0.49f && currentAnimatorState.normalizedTime < 0.59f) //attack only when on halfway of animation
            && !this.isZombieAttacked
            && currentAnimatorState.IsName("attack")) //is state == attack
        {
            // play attack
            // because this method will executed after raycast to player (computing angle)
            // we can assume that AI has clear way to hit player and do damage
            // **But we should look and watch it to prevent bugs
            this.playerClass.Health -= this.damagePerHit; // decrease the damage done by AI hit            
            this.isZombieAttacked = true;// zombie made his move (attacked)
        }
    }

    //checks next destination
    private void checkNextDestination()
    {
        if (this.aiSensors.isTargetInSpot(this.targetTransform)) // check if player on spot of zombie
        {
            this.isInChasingMode = true;
            this.attackIfCan();//attack if can
            this.setNextDestination(true); // set next destination
            this.isInChasingMode = false; // reset chasing (check next frame)

        }
        else if ((!this.navAgent.hasPath && !this.navAgent.pathPending)
            || (this.navAgent.pathStatus == NavMeshPathStatus.PathInvalid) || this.lastPlayerTransform != null)
        {
            this.setNextDestination(true); // set next destination
        }
        else // 
        {
            this.setNextDestination(false); // stay on the current path
        }
    }

    // setting next destination of AI
    private void setNextDestination(bool addNewDestination)
    {
        if(this.isInChasingMode) // check if need to chase after target
        {
            this.navAgent.destination = this.targetTransform.position;
            this.lastPlayerTransform =this.targetTransform; // set last transform of player
            this.lastPlayerPosition = this.targetTransform.position;// set last position
            
            //this.navAgent.destination = this.lastTargetPos.position; // override nav's current destination
            return;//end method
            
        }
        Transform transformOfNextWaypoint = this.waypointQueue.Peek();// get temp Transform

        if (this.lastPlayerTransform != null && addNewDestination) // lost player but go to next pos
        {
            this.addWaypointToFrontOfQueue(this.lastPlayerTransform); // add player's position to front of queue
            this.lastPlayerTransform = null; //reset last player location
        }       
        else if (addNewDestination) 
        {
            if (transformOfNextWaypoint.tag != "Waypoint")
                this.waypointQueue.Dequeue();// remove last player position
            this.waypointQueue.Enqueue(this.waypointQueue.Dequeue());//put to end of queue and prepare next destintaion
        }
        if(transformOfNextWaypoint.tag == "Waypoint")//check if it a waypoint
            this.navAgent.destination = transformOfNextWaypoint.position; // set nav agent to next destination
        else // last position of player
        {
            this.navAgent.destination = this.lastPlayerPosition; // go to last player position
        }

        //print("On the way to: ("+this.waypointQueue.Count+")"+this.waypointQueue.Peek().name);

        //int step = 0;
        //if (addNewDestination) // we need add new destination
        //    step = 1;

        //int nextDestinationIndex;
        //if (this.currentDestinationIndex + step >= this.wayPointList.Count) // check if bigger than list size
        //{
        //    nextDestinationIndex = 0; // start from 0
        //}
        //else
        //{
        //    nextDestinationIndex = this.currentDestinationIndex + step;
        //}
        ////Debug.Log("Agent: " + this.gameObject.name.ToString() + " on the way to: " + nextDestinationIndex);
        //Transform destinationTransform = this.wayPointList[nextDestinationIndex];
        //if (destinationTransform != null) // check if there is Next destination
        //{
        //    this.currentDestinationIndex = nextDestinationIndex;
        //    this.navAgent.destination = destinationTransform.position; // set new destination to the mesh agent
        //    return;
        //}

        //Debug.Log("Agent: " + this.gameObject.name.ToString() + " didn't fount a destination");
        ////play idle animation
        //this.currentDestinationIndex++; // we didn't found destiantion
    }


    //calculating velocity and angle of AI moving and passing to locomotion
    private void calculateVelocityAndAngle()
    {

        // Transform agents desired velocity into local space
        Vector3 localDesiredVelocity = transform.InverseTransformVector(this.navAgent.desiredVelocity);
        // Get angle in degrees we need to turn to reach the desired velocity direction
        this.angle = Mathf.Atan2(localDesiredVelocity.x, localDesiredVelocity.z) * Mathf.Rad2Deg;

        // Smoothly interpolate towards the new angle
        this.smoothAngel = Mathf.MoveTowardsAngle(smoothAngel, this.angle, 80.0f * Time.deltaTime);
        this.animator.SetFloat("angle", this.smoothAngel); // set angel in animator controller
        this.animator.SetFloat("speed", localDesiredVelocity.z, 0.1f, Time.deltaTime); // set speed parameter

        if (this.navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
        {
            Quaternion lookRotation = Quaternion.LookRotation(this.navAgent.desiredVelocity, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5.0f * Time.deltaTime);
        }
    }

    // drawing rays in the scene to better analyze
    private void testAIdetection()
    {
        Vector3 vectorToTarget = targetTransform.position - this.zombiesHeadPos.transform.position;
        Ray ray = new Ray(this.zombiesHeadPos.transform.position, vectorToTarget);
        RaycastHit hit;
        //test
        if (Physics.Raycast(ray, out hit))
        {

            vectorToTarget.y = this.zombiesHeadPos.transform.forward.y;
            ray = new Ray(this.zombiesHeadPos.transform.position, vectorToTarget);
            
            Debug.DrawRay(this.zombiesHeadPos.position, ray.direction, Color.red);
            Debug.DrawRay(this.zombiesHeadPos.position,
                 Quaternion.Euler(0, this.maxAngleOfDetection, 0) * zombiesHeadPos.forward
                , Color.blue);
            Debug.DrawRay(this.zombiesHeadPos.position,
                 Quaternion.Euler(0, -this.maxAngleOfDetection, 0) * zombiesHeadPos.forward
                , Color.blue);
            Debug.DrawRay(this.zombiesHeadPos.position, this.zombiesHeadPos.forward.normalized, Color.black);
            //Debug.Log("Angel: " + Vector3.Angle(this.zombiesHeadPos.forward.normalized,
            //    ray.direction));
            //print("New Ray: " + hit.collider.gameObject.name);
            //if (hit.collider.tag == "Player")
            //{
            //    print("I Hit Player");
            //}
        }
    }

    // add Transform to front of queue
    private void addWaypointToFrontOfQueue(Transform waypointToAdd)
    {
        Queue<Transform> tempQueue = new Queue<Transform>(); // temp queue
        while(this.waypointQueue.Count > 0)
        {
            tempQueue.Enqueue(this.waypointQueue.Dequeue());
        }
        this.waypointQueue.Enqueue(waypointToAdd); // insert object to fron of queue
        while (tempQueue.Count > 0)
        {
            this.waypointQueue.Enqueue(tempQueue.Dequeue());
        }
    }


}
