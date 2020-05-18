using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class RollerAgent : Agent
{
    public Transform target;
    public float speed = 10f;

    private Rigidbody rBody;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    // Dealing with resetting the environment/agent
    public override void AgentReset()
    {
        // if Agent falls down
        if (transform.position.y < 0) 
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.position = new Vector3(0, 0.5f, 0);
        }

        // move target to new spot
        target.position = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
        base.AgentReset();
    }

    // Collecting Observations and sending it to Brain
    public override void CollectObservations()
    {
        AddVectorObs(target.position);
        AddVectorObs(transform.position);

        AddVectorObs(rBody.velocity.x);
        AddVectorObs(rBody.velocity.z);

        base.CollectObservations();
    }

    // Receiving Decision/Actions and assigning reward
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        rBody.AddForce(controlSignal * speed);

        // Rewards
        float distanceToTarget = Vector3.Distance(transform.position,
                                                  target.position);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            Done();
        }

        // Fell off platform
        if (transform.position.y < 0)
        {
            Done();
        }

    }


}
