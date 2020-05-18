using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
using UnityEngine.SceneManagement;

public class RLController : Agent
{
    public Transform target;
    public float speed = 10f;
    public int healthOnHazard = 20;
    public int healthOnPickup = 20;

    public List<GameObject> spikesList;
    private List<GameObject> totalHealthItems;
    private List<GameObject> totalHazard;

    private Rigidbody rBody;
    private List<GameObject> collectibleList;
    
    private bool indexMoved;
    private SphereCollider areaOfDetection;
    private float maxEpisodeLength;
    private bool timePassed = false;
    private bool hitHazard = false;
    private int hitHazardInstance = 0;
    private int hitCount = 0;

    private void Awake()
    {
        spikesList = new List<GameObject>();
        
        GameObject spikeOne = new GameObject();
        GameObject spikeTwo = new GameObject();
        // Initialisation before overwriting on each subsequent frame after first
        spikesList.Insert(0, spikeOne);
        spikesList.Insert(1, spikeTwo);
    }

    // first frame call
    void Start() 
    {
        rBody = GetComponent<Rigidbody>();
        collectibleList = new List<GameObject>();
        maxEpisodeLength = Time.time + 480;
        areaOfDetection = gameObject.GetComponent<SphereCollider>();
        indexMoved = false;

        // Get all health items
        totalHealthItems = new List<GameObject>(GameObject.FindGameObjectsWithTag("Collectible"));
        totalHazard = new List<GameObject>(GameObject.FindGameObjectsWithTag("Hazard"));

        Debug.LogWarning("Total Health Items - " + totalHealthItems.Count);
        Debug.LogWarning("Total Hazard - " + totalHazard.Count);

        if (collectibleList.Count > 0) //ensure target at start
        target = GetNearestCollectible(collectibleList).transform;
    }

    private void Update()
    {
        if (!(GameObject.FindGameObjectWithTag("Collectible")))
        {
            Time.timeScale = 0; // comment for training
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("SampleScene");
        }
        //Debug.LogWarning("RL Health is " + GameController.Instance.GetRLHealth());
        if (collectibleList.Count > 0) // might give issue if there is no health object in range, it will respawn everything
        {
            target = GetNearestCollectible(collectibleList).transform;
        }
        if (!(GameObject.FindGameObjectWithTag("Collectible"))) // return true only if a single tagged object is active
        {
            // Re-enable all collectibles
            foreach (GameObject item in totalHealthItems)
            {
                item.gameObject.GetComponent<BoxCollider>().enabled = true;
                item.SetActive(true);
            }
            Debug.LogError("Restarted Exercise");
            /*
            SetReward(1f); // was set
            Done();
            */
            //Done();
        }
        float timeLeft = maxEpisodeLength - Time.time;
        if (timeLeft < 0 && (GameObject.FindGameObjectWithTag("Collectible"))) // ensure a time limit for each episode
        {
            timePassed = true;   
            maxEpisodeLength = Time.time + 480;
        }

    }

    public override void AgentReset()
    {
        if (transform.position.y < 0)
        {
            // If the Agent fell, zero its momentum
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.position = new Vector3(-40, 1, -5);
            //transform.position = new Vector3(0, 0.5f, 0); Enable only for training
        }

        // Move the target to a new spot
        // Enable these methods for training
          
        //ResetSpikes();
        //ResetTarget();

    }

    public override void CollectObservations()
    {
        // Target and Agent positions
        AddVectorObs(target.position); // Vector Space 3 // issue as target set adter first frame fixed update should fix this?
        AddVectorObs(transform.position); // Vector Space 3
        
        // Spikes Position
        foreach(GameObject spike in spikesList) // 3 + 3
        {
            AddVectorObs(spike.transform.position); // only two spikes at a time.. closest spikes
        }

        // Agent velocity
        AddVectorObs(rBody.velocity.x); // Vector Space 1
        AddVectorObs(rBody.velocity.z); // Vector Space 1
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        rBody.AddForce(controlSignal * speed);

        // Rewards
        //float distanceToTarget = Vector3.Distance(this.transform.position,
        //                                          target.position);

        /*
        // Reached target
        if (distanceToTarget < 0.5f)
        {
            SetReward(1f);
            Done();
        }
        */
        
        // Fell off platform
        if (transform.position.y < 0)
        {
            Done();
        }
        if (!(GameObject.FindGameObjectWithTag("Collectible")))
        {
            AddReward(5f); // was set
            Done();
        }
        /*
        if (hitHazard)
        {
            hitHazard = false;
             //0.025 
            //Done();
        }
        */
        
        /*
        if (timePassed)
        {
            AddReward(-0.0002f);
            Done();
            timePassed = false;
        }
        ^/

        /*
        if (GameController.Instance.GetRLHealth() <= 0)
        {
            SetReward(-1f);
            Done(); // resets agent not level
        }
        */
        /* Game Controller handling original reset level .. should we reset level??
        if(GameController.Instance.GetRLHealth() <= 0)
        {
            SetReward(-1f);
            Done();
        }
        */

    }

    private void ResetSpikes()
    {
        spikesList[0].transform.position = new Vector3(Random.Range(4f, 6f), 0.1f, Random.Range(2f, 4f));
        spikesList[1].transform.position = new Vector3(Random.Range(-6f, -2f), 0.1f, Random.Range(0f, 6f));
        float distanceSpike = Vector3.Distance(spikesList[0].transform.position, spikesList[1].transform.position);
        float distanceAgent = Vector3.Distance(spikesList[0].transform.position, transform.position);
        float distanceAgentSpikeTwo = Vector3.Distance(spikesList[1].transform.position, transform.position);
        if (distanceSpike < 3f || distanceAgent < 2f || distanceAgentSpikeTwo < 2f)
        {
            ResetSpikes();
        }
    }

    private void ResetTarget()
    {
        target = GetNearestCollectible(collectibleList).transform;
        target.position = new Vector3(Random.Range(-1f, 3f), 0.1f, Random.Range(-5f, 5f));
        foreach (GameObject spike in spikesList)
        {
            if (Vector3.Distance(target.position, spike.transform.position) < 2f)
            {
                ResetTarget();
            }
        }
        /*
        target.position = new Vector3(Random.Range(-1f, 3f), 0.1f, Random.Range(-5f, 5f));
        foreach(GameObject spike in spikes)
        {
            if (Vector3.Distance(target.position, spike.transform.position) < 2f)
            {
                ResetTarget();
            }
        }
        */
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        ContactPoint[] contactPoints = collision.contacts;
        for (int contactIndex = 0; contactIndex < contactPoints.Length; contactIndex++)
        {
            ContactPoint cp = contactPoints[0];
            Collider otherObjectCollider = cp.otherCollider;

            //Debug.Log(collision.gameObject.name);
            if (otherObjectCollider.gameObject.CompareTag("Hazard") && hitCount == 0)
            {
                GameController.Instance.ReceiveDamage(gameObject, healthOnHazard);
                hitCount++;
                // Agent 14
                //AddReward(-1f / totalHazard.Count);
                //AddReward(-0.025f); // divide 1/noOfSpikes
                //Done();
                AddReward(-1f / totalHazard.Count);
                //hitHazard = true;
                return; // stop handling collisions
            }
            if (otherObjectCollider.gameObject.CompareTag("Collectible"))
            {
                
                GameController.Instance.ReceiveHealth(gameObject, healthOnPickup);
                //areaOfDetection.radius -= healthOnPickup; disabled for training purposes

                if (collectibleList.Contains(otherObjectCollider.gameObject))
                {
                    //Debug.Log("I have eaten " + collision.gameObject.name);
                    collectibleList.Remove(otherObjectCollider.gameObject);
                    //Debug.Log("And there are now " + collectibleList.Count + " in list.");
                    otherObjectCollider.gameObject.GetComponent<BoxCollider>().enabled = false;
                    otherObjectCollider.gameObject.SetActive(false);
                    //Destroy(collision.gameObject);
                    //AddReward(0.8f);
                    //AddReward(1f / totalHealthItems.Count);
                    Debug.LogError("Consumed Health!");
                }
                // remove from list then destroy, otherwise null pointer exception
                //SetReward(1f);
                //AddReward(0.045f);
                // Agent 14
                
                //Done();
                
                return;
            }

            if (otherObjectCollider.gameObject.CompareTag("Wall"))
            {
                
                // Agent 14
                AddReward(-0.2f);
                Done();
                // Nothing was here
                //Done();
                return;
            }

        }
        hitCount = 0;
        

        /*
             if (collision.gameObject.tag == "Collectible")
        {
            GameController.Instance.ReceiveHealth(gameObject, healthOnPickup);
            areaOfDetection.radius -= healthOnPickup;
            for(int i = 0; i < collectibleList.Count; i++)
            {
                if (collision.gameObject.GetInstanceID() == collectibleList[i].gameObject.GetInstanceID())
                {
                    Debug.Log("I have eaten " + collectibleList[i].gameObject.name);
                    collectibleList.Remove(collectibleList[i].gameObject);
                    Debug.Log("And there are now " + collectibleList.Count + " in list.");
                    Destroy(collision.gameObject);
                }
            }
             // remove from list then destroy, otherwise null pointer exception
        }
        if (collision.gameObject.tag == "Hazard")
        {
            GameController.Instance.ReceiveDamage(gameObject, healthOnHazard);
        }
        UnityEngine.Debug.Log("RBS collided with - " + collision.gameObject.tag + " called " + collision.gameObject.name); 
        */

    }

    private void OnTriggerEnter(Collider other)
    {
        
        // make sure that object is not already in list before adding.
        // create array to store all collectible objects within sphere, calculate distance to each one go to nearest
        if (other.gameObject.tag == "Collectible" && !collectibleList.Contains(other.gameObject))
        {
            collectibleList.Add(other.gameObject);
            // Go to collectible here
            // if we detect something go to raycast?
            //transform.Rotate(0, GetRotation(GetNearest(collectibleList).transform.position), 0); // lerp for smoothness?
            // If something detection in ray get new direction always, but this should be handled in Update <-
            //Debug.Log("Enter Trigger - Currently there is/are " + collectibleList.Count + " collectibles within reach");
        }
        if (other.gameObject.tag == "Hazard" && !spikesList.Contains(other.gameObject)) // can be improved use breakpoints
        {
            if (spikesList.Count < 2)
            {
                spikesList.Add(other.gameObject);
            }
            if (other.gameObject.tag == "Hazard" && !spikesList.Contains(other.gameObject) && spikesList.Count >= 2)
            {
                // add them to pool?
                // replace on current .. first replace 1, then replace 2, then replace 1 etc..
                spikesList.RemoveAt(0); // this will make all objects move one place down
                spikesList.Insert(1, other.gameObject); // this works because of Linq
                indexMoved = true;
                return;
            }
            if (indexMoved)
            {
                spikesList.RemoveAt(1);
                spikesList.Insert(1, other.gameObject);
                indexMoved = false;
            }
        }
        
        // we have to deal with object who are actually eaten up // this was completed near Destroy Collider


    }

    private void OnTriggerExit(Collider other)
    {

        if (other.gameObject.tag == "Collectible" && collectibleList.Contains(other.gameObject))
        {
            collectibleList.Remove(other.gameObject);
            //Debug.Log("Exit Trigger - Currently there is/are " + collectibleList.Count + " collectibles within reach");
        } // ^ should this be really here? Answer - Yes, otherwise we will have a problem with for loop in ontriggerenter

        // do not remove, replace. we need to have two at all times // this might be tricky since using triggers
        // perhaps use bools .. two lists? pool
        // never remove, replace last known spike with new known spike
        /*
        if (other.gameObject.tag == "Hazard" && spikes.Contains(other.gameObject)) 
        { 
            spikes.Remove(other.gameObject); // could cause problem since we need two spikes at a time
            /*
            if(spikes.Count < 2)
            {

            }
            
            Debug.Log("Exit Trigger - Currently there is/are " + spikes.Count + " collectibles within reach");
        } // ^ should this be really here? Answer - Yes, otherwise we will have a problem with for loop in ontriggerenter
        */
    }

    private GameObject GetNearestCollectible(List<GameObject> collectibles)
    {
        // Find nearest item.
        GameObject nearest = null;
        float distance = 0;

        for (int i = 0; i < collectibles.Count; i++)
        {
            float tempDistance = Vector3.Distance(transform.position, collectibles[i].transform.position);
            if (nearest == null || tempDistance < distance)
            {
                nearest = collectibles[i];
                distance = tempDistance;
            }
        }

        return nearest;
        /*
        // Remove from list.
        items.Remove(nearest);

        // Destroy object.
        Destroy(nearest.gameObject);
        */
    }

    /* Design Choice - spikes, first two in trigger, remove from trigger on exit

    private GameObject GetNearestSpikes(List<GameObject> spikes)
    {
        // Find nearest item.
        GameObject nearest = null;
        float distance = 0;

        for (int i = 0; i < spikes.Count; i++)
        {
            float tempDistance = Vector3.Distance(transform.position, spikes[i].transform.position);
            if (nearest == null || tempDistance < distance)
            {
                nearest = spikes[i];
                distance = tempDistance;
            }
        }

        return nearest;
        /*
        // Remove from list.
        items.Remove(nearest);

        // Destroy object.
        Destroy(nearest.gameObject);
        
    }

    */

    /*

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
            transform.position = new Vector3(0, 1f, 0);
        }

        // move target to new spot
        target.position = new Vector3(Random.Range(-5f, 9), 0.1f, Random.Range(-9f, 9f));
        foreach (GameObject spike in spikes)
        {
            spike.transform.position = new Vector3(Random.Range(-5f, 9), 0.1f, Random.Range(-9f, 9f));
        }
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
        if (distanceToTarget < 0.25f)
        {
            SetReward(0.5f);
            Done();
        }

        // Fell off platform
        if (transform.position.y < 0)
        {
            Done();
        }

    }
    */
    /*
    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Collectible")
        {
            consumedHealth = true;
            Destroy(collision.gameObject);
            /*
            GameController.Instance.ReceiveHealth(gameObject, healthOnPickup);
            areaOfDetection.radius -= healthOnPickup;
            for (int i = 0; i < collectibleList.Count; i++)
            {
                if (collision.gameObject.GetInstanceID() == collectibleList[i].gameObject.GetInstanceID())
                {
                    Debug.Log("I have eaten " + collectibleList[i].gameObject.name);
                    collectibleList.Remove(collectibleList[i].gameObject);
                    Debug.Log("And there are now " + collectibleList.Count + " in list.");
                    Destroy(collision.gameObject);
                }
            }
            // remove from list then destroy, otherwise null pointer exception
            
        }
        if (collision.gameObject.tag == "Hazard")
        {
            GameController.Instance.ReceiveDamage(gameObject, healthOnHazard);
            SetReward(-1f);
        }
        UnityEngine.Debug.Log("RBS collided with - " + collision.gameObject.tag + " called " + collision.gameObject.name);

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Collectible")
        consumedHealth = false;
    }
    */
}
