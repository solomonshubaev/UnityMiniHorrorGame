using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
// ----------------------------------------------------------
// CLASS	:	NavAgentRootMotion
// DESC		:	Behaviour to test Unity's NavMeshAgent with
//				Animator component using root motion
// ----------------------------------------------------------
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class TutorialAgent : MonoBehaviour
{
	// Inspector Assigned Variable
	[SerializeField] private List<Transform> wayPointList;
	public int CurrentIndex = 0;
	public bool HasPath = false;
	public bool PathPending = false;
	public bool PathStale = false;
	public UnityEngine.AI.NavMeshPathStatus PathStatus = UnityEngine.AI.NavMeshPathStatus.PathInvalid;
	public AnimationCurve JumpCurve = new AnimationCurve();
	public bool MixedMode = true;

	// Private Members
	private UnityEngine.AI.NavMeshAgent _navAgent = null;
	private Animator _animator = null;
	private float _smoothAngle = 0.0f;

	// -----------------------------------------------------
	// Name :	Start
	// Desc	:	Cache MavMeshAgent and set initial 
	//			destination.
	// -----------------------------------------------------
	void Start()
	{
		// Cache NavMeshAgent Reference
		_navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

		_animator = GetComponent<Animator>();

		// Turn off auto-update of rotation
		/*_navAgent.updatePosition = false;*/
		_navAgent.updateRotation = false;

		

		// Set first waypoint
		SetNextDestination(false);
	}

	// -----------------------------------------------------
	// Name	:	SetNextDestination
	// Desc	:	Optionally increments the current waypoint
	//			index and then sets the next destination
	//			for the agent to head towards.
	// -----------------------------------------------------
	void SetNextDestination(bool increment)
	{
		// If no network return

		// Calculatehow much the current waypoint index needs to be incremented
		int incStep = increment ? 1 : 0;
		Transform nextWaypointTransform = null;

		// Calculate index of next waypoint factoring in the increment with wrap-around and fetch waypoint 
		int nextWaypoint = (CurrentIndex + incStep >= wayPointList.Count) ? 0 : CurrentIndex + incStep;
		nextWaypointTransform = wayPointList[nextWaypoint];

		// Assuming we have a valid waypoint transform
		if (nextWaypointTransform != null)
		{
			// Update the current waypoint index, assign its position as the NavMeshAgents
			// Destination and then return
			CurrentIndex = nextWaypoint;
			_navAgent.destination = nextWaypointTransform.position;
			return;
		}

		// We did not find a valid waypoint in the list for this iteration
		CurrentIndex = nextWaypoint;
	}

	// ---------------------------------------------------------
	// Name	:	Update
	// Desc	:	Called each frame by Unity
	// ---------------------------------------------------------
	void Update()
	{

		// Copy NavMeshAgents state into inspector visible variables
		HasPath = _navAgent.hasPath;
		PathPending = _navAgent.pathPending;
		PathStale = _navAgent.isPathStale;
		PathStatus = _navAgent.pathStatus;

		// Transform agents desired velocity into local space
		Vector3 localDesiredVelocity = transform.InverseTransformVector(_navAgent.desiredVelocity);

		// Get angle in degrees we need to turn to reach the desired velocity direction
		float angle = Mathf.Atan2(localDesiredVelocity.x, localDesiredVelocity.z) * Mathf.Rad2Deg;

		// Smoothly interpolate towards the new angle
		_smoothAngle = Mathf.MoveTowardsAngle(_smoothAngle, angle, 80.0f * Time.deltaTime);

		// Speed is simply the amount of desired velocity projected onto our own forward vector
		float speed = localDesiredVelocity.z;

		// Set animator parameters
		_animator.SetFloat("angle", _smoothAngle);
		_animator.SetFloat("speed", speed, 0.1f, Time.deltaTime);

		if (_navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
		{
			if (!MixedMode ||
				(MixedMode && Mathf.Abs(angle) < 80.0f && _animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion")))
			{
				Quaternion lookRotation = Quaternion.LookRotation(_navAgent.desiredVelocity, Vector3.up);
				transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5.0f * Time.deltaTime);
			}
		}

		// If agent is on an offmesh link then perform a jump
		/*if (_navAgent.isOnOffMeshLink)
		{
			StartCoroutine( Jump( 1.0f) );
			return;
		}*/

		// If we don't have a path and one isn't pending then set the next
		// waypoint as the target, otherwise if path is stale regenerate path
		if ((_navAgent.remainingDistance <= _navAgent.stoppingDistance && !PathPending) || PathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid /*|| PathStatus==NavMeshPathStatus.PathPartial*/)
		{
			SetNextDestination(true);
		}
		else
		if (_navAgent.isPathStale)
			SetNextDestination(false);

	}

	// ----------------------------------------------------------
	// Name	:	OnAnimatorMove
	// Desc	:	Called by Unity to allow application to process
	//			and apply root motion
	// ----------------------------------------------------------
	void OnAnimatorMove()
	{
		// If we are in mixed mode and we are not in the Locomotion state then apply root rotation
		if (MixedMode && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion"))
			transform.rotation = _animator.rootRotation;

		// Override Agent's velocity with the velocity of the root motion
		_navAgent.velocity = _animator.deltaPosition / Time.deltaTime;
	}

	// ---------------------------------------------------------
	// Name	:	Jump
	// Desc	:	Manual OffMeshLInk traversal using an Animation
	//			Curve to control agent height.
	// ---------------------------------------------------------
}
