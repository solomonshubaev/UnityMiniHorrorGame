using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiSensors
{
    private float maxAngleOfDetecting;
    private Vector3 vectorForward;
    private Transform headTransform;
    private float maxDistance;

    public AiSensors(float maxAngleOfDetecting, Vector3 vectorForward,
        Transform transform, float maxDistance)
    {
        this.maxAngleOfDetecting = maxAngleOfDetecting;
        this.vectorForward = vectorForward;
        this.headTransform = transform;
        this.maxDistance = maxDistance;
    }

    public float MaxAngleOfDetecting { get => maxAngleOfDetecting; set => maxAngleOfDetecting = value; }
    public Vector3 VectorForward { get => vectorForward; set => vectorForward = value; }
    public Transform Transform { get => headTransform; set => headTransform = value; }
    public float MaxDistance { get => maxDistance; set => maxDistance = value; }

    /// returns true if target detected
    public bool isTargetInSpot(Transform targetTransform)
    {
        float angle = this.getAngleToTransform(targetTransform);
        if (angle >= 0 && angle <= maxAngleOfDetecting) // if target in the spot
            return true;
        return false;
    }
    public float getAngleToTransform(Transform targetTransform)
    {
        Vector3 vectorToTarget = targetTransform.position - this.headTransform.position;
        Ray ray = new Ray(this.headTransform.position, vectorToTarget);
        RaycastHit hit;

        if (Physics.Raycast(ray , out hit))
        {
            if (hit.collider.tag == "Player")
            {
                vectorToTarget.y = this.headTransform.forward.y; // set y componenet yo same us head's y
                ray = new Ray(this.headTransform.position, vectorToTarget);
                float angle = Vector3.Angle(this.headTransform.forward.normalized,
                ray.direction); //calculate the angle
                angle = Mathf.Abs(angle); // get absolute angle

                return angle;//we calculate the angle now return        
            }
        }


        return Definitions.ERROR_VALUE; //return error
    
    }
}
