using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RBSController : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 100f;
    public int healthOnPickup = 10;
    public int healthOnHazard = 10;
    public int detectionArea = 4;
    public int inverseLerpOffset = 50;
    public float nextTargetDelay = 4;

    private SphereCollider areaOfDetection;
    private BoxCollider cubeCollider;
    private Rigidbody rb;

    
    private Vector3 startDirection;
    private Vector3 targetDirection;
    private List<GameObject> collectibleList;

    private float startDetectionRadius;
    private float finalAreaOfDetection;
    private float fovAngle;
    private float vectorScale;
    private bool leftHazard, centreHazard, rightHazard = false;

    // Start is called before the first frame update
    void Start()
    {
        areaOfDetection = gameObject.GetComponent<SphereCollider>();
        rb = gameObject.GetComponent<Rigidbody>();
        cubeCollider = gameObject.GetComponentInChildren<BoxCollider>();

        startDetectionRadius = areaOfDetection.radius;
        fovAngle = 90f;
        vectorScale = 1.25f;

        collectibleList = new List<GameObject>();

        
        Debug.LogWarning("Debug Log on Rays");
    }

    // Update is called once per frame
    void Update()
    {
        if (!(GameObject.FindGameObjectWithTag("Collectible")))
        {
            Time.timeScale = 0;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("SampleScene");
        }
        // Area of Detection
        // Inverse Lerp with lower values of deduction rate does not work
        finalAreaOfDetection = Mathf.InverseLerp(startDetectionRadius, GameController.Instance.GetRBSHealth(), 10);
        areaOfDetection.radius = finalAreaOfDetection * inverseLerpOffset;

        // Limit AreaOfDetection
        if (areaOfDetection.radius >= detectionArea)
        {
            areaOfDetection.radius = detectionArea;
        }

        // Movement

        // if something detected, change direction
        // else move forward in current direction and angle. // method, store currentDirection vector, supply
        // current direction vector and get new direction vector from function
        transform.position += transform.forward * speed * Time.deltaTime; // replace transform forward with random direction
        //transform.Rotate(0, rotationSpeed, 0); // every frame rotating by 30 // not ideal issue here
        // rotate towards new position?
        
        //Debug.Log(collectibleList.)

        Vector3 leftRayRotation = Quaternion.AngleAxis(-fovAngle, transform.up) * transform.forward;
        Vector3 rightRayRotation = Quaternion.AngleAxis(fovAngle, transform.up) * transform.forward;

        // RayCast to new target which will be collectible -> on collision enter?
        
        Ray rayLeft = new Ray(transform.position + transform.forward, leftRayRotation.normalized * vectorScale); // left
        Ray laserRay = new Ray(transform.position + transform.forward, transform.forward * vectorScale); // centre
        Ray rayRight = new Ray(transform.position + transform.forward, rightRayRotation.normalized * vectorScale); // right

        //Debug.DrawRay(transform.position + transform.forward, transform.forward * areaOfDetection.radius, Color.black); // centre
        Debug.DrawRay(transform.position + transform.forward, rayLeft.direction.normalized * vectorScale, Color.red); // left
        Debug.DrawRay(transform.position + transform.forward, transform.forward * vectorScale, Color.yellow); // centre
        Debug.DrawRay(transform.position + transform.forward, rayRight.direction.normalized * vectorScale, Color.red); // right

        if (Physics.Raycast(rayLeft, out RaycastHit leftHit, rayLeft.direction.normalized.magnitude * vectorScale))
        {
            switch (leftHit.collider.tag)
            {
                case "Collectible":
                    leftHazard = false;
                    break;
                case "Hazard":
                    
                    Debug.LogError("Left Ray said there is a hazard, change direction");

                    Vector3 newDir;
                    Vector3 curDir = gameObject.transform.TransformDirection(transform.forward);
                    newDir = Vector3.Reflect(curDir, leftHit.normal);
                    transform.rotation = Quaternion.FromToRotation(Vector3.forward, newDir);
                    leftHazard = true;
                    break;
                case "Wall":
                    Debug.LogWarning("This is a wall, change direction");

                    Vector3 newDirFromWall;
                    Vector3 curDirToWall = gameObject.transform.TransformDirection(transform.forward);
                    newDirFromWall = Vector3.Reflect(curDirToWall, leftHit.normal);
                    transform.rotation = Quaternion.FromToRotation(Vector3.forward, newDirFromWall);
                    //GetNewDirection(hit);
                    // call rotate method
                    break;
                default:
                    
                    // do nothing
                    break;
            }
        }
        else
        {
            StartCoroutine(Cooldown(nextTargetDelay));
        }

        if (Physics.Raycast(rayRight, out RaycastHit rightHit, rayRight.direction.normalized.magnitude * vectorScale))
        {
            switch (rightHit.collider.tag)
            {
                case "Collectible":
                    rightHazard = false;
                    break;
                case "Hazard":
                    
                    Debug.LogError("Right Ray said there is a hazard, change direction");

                    Vector3 newDir;
                    Vector3 curDir = gameObject.transform.TransformDirection(transform.forward);
                    newDir = Vector3.Reflect(curDir, rightHit.normal);
                    transform.rotation = Quaternion.FromToRotation(Vector3.forward, newDir);
                    rightHazard = true;
                    break;
                case "Wall":
                    Debug.LogWarning("This is a wall, change direction");

                    Vector3 newDirFromWall;
                    Vector3 curDirToWall = gameObject.transform.TransformDirection(transform.forward);
                    newDirFromWall = Vector3.Reflect(curDirToWall, rightHit.normal);
                    transform.rotation = Quaternion.FromToRotation(Vector3.forward, newDirFromWall);
                    //GetNewDirection(hit);
                    // call rotate method
                    break;
                default:
                    
                    // do nothing
                    break;
            }
        }
        else
        {
            StartCoroutine(Cooldown(nextTargetDelay));
        }

        if (Physics.Raycast(laserRay, out RaycastHit centreHit, laserRay.direction.normalized.magnitude * vectorScale))
        {
            switch (centreHit.collider.tag)
            {
                case "Collectible":
                    centreHazard = false;
                    //Debug.LogWarning("Wohoo I want to eat! This is handled by NextTarget function.");
                    
                    // change target to point to this direction, calculate distance between object and go to that location
                    // if encounter hazard along the way, keep object in mind and get a set of new directions based on rays

                    // same as below, if we encounter collectible with ray first, then change to that angle
                    // or else just increase speed go to that direction.

                    // go forward, increase speed
                    break;
                case "Hazard":
                    Debug.LogError("Centre Ray - Oh no! Avoid at all costs and get new path!");

                    Vector3 newDirFromHazard;
                    Vector3 curDirToHazard = gameObject.transform.TransformDirection(transform.forward);
                    newDirFromHazard = Vector3.Reflect(curDirToHazard, centreHit.normal);
                    transform.rotation = Quaternion.FromToRotation(Vector3.forward, newDirFromHazard);
                    centreHazard = true;
                    // check if there is hazard in left or right rays, otherwise take and override the nexttarget direction

                    // invoke method to get angle(0, 45 , 90 , 135, 180, 225, 270, 315)(based on ray) which has no hazard or has collectibles in its path
                    // for method get current angle and based on this draw rays to check -45, 45 possibly recursion?

                    // call rotate method

                    break;
                case "Wall":
                    Debug.LogWarning("This is a wall, change direction");

                    Vector3 newDirFromWall;
                    Vector3 curDirToWall = gameObject.transform.TransformDirection(transform.forward);
                    newDirFromWall = Vector3.Reflect(curDirToWall, centreHit.normal);
                    transform.rotation = Quaternion.FromToRotation(Vector3.forward, newDirFromWall);
                    //GetNewDirection(hit);
                    // call rotate method
                    break;
               
                default:
                    
                    // this was creating issue as default is called on every switch
                    //speed /= speedMultiplier;
                    // keep moving in same direction, reset speed
                    // UNLESS you find a new thing to eat, then go to new direction

                    // we can create complexity here if we have more time, and utilise spherecollider
                    break;
            }
        }
        else
        {
            StartCoroutine(Cooldown(nextTargetDelay));
            //centreHazard = false; -> cannot do this here as it will instantly try again to go to target
        }

        
       
        /*

        if (leftHit.collider.CompareTag("Hazard") && !leftHit.collider.CompareTag(null))
        {
            transform.rotation = Quaternion.FromToRotation(rayLeft.direction, laserRay.direction);
        }
        if (rightHit.collider.CompareTag("Hazard") && !rightHit.collider.CompareTag(null))
        {
            transform.rotation = Quaternion.FromToRotation(rayRight.direction, laserRay.direction);
        }
        if (centreHit.collider.CompareTag("Hazard") && !centreHit.collider.CompareTag(null))
        {
            if (Random.Range(0, 10) > 5)
            {
                transform.rotation = Quaternion.FromToRotation(laserRay.direction, rayLeft.direction);
            }
            else
            {
                transform.rotation = Quaternion.FromToRotation(laserRay.direction, rayRight.direction);
            }
        }
        */

        /* It is broken
        if (collectibleDetected)
        {
            if (GetNearest(collectibleList))
            {
                transform.position = Vector3.MoveTowards(transform.position, GetNearest(collectibleList).transform.position, Vector3.Distance(transform.position, GetNearest(collectibleList).transform.position));
            }
            else
            {
                // do nothing
            }
        }
        */
        // Pseudo

        // if ray cast of one unit there is nothing in front of me, move to that direction.
        // if raycast hit object that is hazard, get a new direction for ray cast and move to that direction.
        // else raycast hit collectible, calculate how many collectibles are within the trigger space, and find shortest distance. set the shortest distance
        // to the new target for the rbs, getting movetowards, rotate towards, while checking that ray targets hit are not hazard, keep finding vectors that are
        // within the target space, range angle + - 15 from current position so calculate difference in position as well.

        //Debug.Log(finalAreaOfDetection);



        // Manual Override 
        /*
        
        transform.Rotate(0, Input.GetAxis("Horizontal") * Time.deltaTime * rotationSpeed, 0);
        transform.Translate(0, 0, Input.GetAxis("Vertical") * Time.deltaTime * speed);
        
        */

    }

    private void FixedUpdate()
    {
        if (collectibleList.Count > 0)
        {
            //&& !centreHazard && !rightHazard && !leftHazard
            // create delay
            //StartCoroutine(Cooldown(nextTargetDelay)); // there is a delay before moving to next target.. can be improved .. delay only on hit from hazard
            if (leftHazard != true || centreHazard != true || rightHazard != true) // these are staying true as there is no reset
            StartCoroutine(Cooldown(nextTargetDelay));
            //NextTarget(); // IENumerator .. to give time to reflect and then call again
        }
        else
        {
            //StopAllCoroutines();
        }
    }

    /* Handled by GetRotation and GetNearest

    private Vector3 GetDirection(Vector3 currentPosition, Vector3 targetPosition, Quaternion currentRotation)
    {
        Vector3 newDirection = Vector3.one;
        // do stuff here
        return newDirection;
    }

    */

    IEnumerator Cooldown(float seconds)
    {
        
        yield return new WaitForSeconds(seconds);
        NextTarget();
        StopCoroutine(Cooldown(seconds));
        StopAllCoroutines(); // solved all problems

    }

    private void OnCollisionEnter(Collision collision)
    {
        
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
            Debug.Log("Enter Trigger - Currently there is/are " + collectibleList.Count + " collectibles within reach");
        }
        else
        {
            // think about putting this in ontriggerstay as well
            // do nothing, this is where it should go back and handle from update
        }
        
        // we have to deal with object who are actually eaten up // this was completed near Destroy Collider

        
    }

    private void NextTarget()
    {
        // add to list here, iterate through the list first to ensure that the object has not already been added.

        Debug.Log("RBS Angle before calling method without 360 " + (transform.rotation.eulerAngles));
        Debug.Log("RBS Angle before calling method " + (transform.rotation.eulerAngles.y - 360f));
        // check instance ID?
        if ((GetNearest(collectibleList)) != null && Mathf.Approximately(GetRotation(GetNearest(collectibleList).transform.position), transform.rotation.eulerAngles.y - 360f) ||
           (GetRotation(GetNearest(collectibleList).transform.position) + 10f >= (transform.rotation.eulerAngles.y - 360f) && 
           (GetRotation(GetNearest(collectibleList).transform.position) - 10f <= (transform.rotation.eulerAngles.y - 360f))))
        {
            // Current rotation towards nearest object position is similar or within 10 degree left/right
            // do nothing

            // could this be causing rotation issues?

            Debug.Log("Do nothing!");
        }
        else
        {
           
            // change angle
            Debug.Log("Change Angle");

            Vector3 targetDir = GetNearest(collectibleList).transform.position - transform.position;
            float step = rotationSpeed * Time.deltaTime;

            //Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
            Quaternion LookAtRotation = Quaternion.LookRotation(targetDir);
            Quaternion LookAtRotationOnly_Y = Quaternion.Euler(transform.rotation.eulerAngles.x, LookAtRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            transform.rotation = LookAtRotationOnly_Y;
            /*
            if (left.collider.CompareTag("Hazard") && !centre.collider.CompareTag("Hazard"))
            {
                transform.rotation = LookAtRotationOnly_Y * Quaternion.AngleAxis(fovAngle, transform.up);
            }    
            else
            {
                transform.rotation = LookAtRotationOnly_Y;
            }
            if (right.collider.CompareTag("Hazard") && !centre.collider.CompareTag("Hazard"))
            {
                transform.rotation = LookAtRotationOnly_Y * Quaternion.AngleAxis(-fovAngle, transform.up);
            }
            else
            {
                transform.rotation = LookAtRotationOnly_Y;
            }
            if (centre.collider.CompareTag("Hazard")) // not working as intended. Think about removing the upper angle modifications
            {                                         // as weird behaviour is killing cube on rotation
                transform.position -= new Vector3(0, 0, transform.forward.z - 5f);
            }
            */
            // centre should reflect back from Physics RayCast Method
          
            //transform.Rotate(0, GetRotation(GetNearest(collectibleList).transform.position), 0);
        }
        Debug.Log("RBS Angle is " + (transform.rotation.eulerAngles.y - 360f));
        Debug.Log("GetNearest is returning this position -" + GetNearest(collectibleList).transform.position);
        Debug.Log("The GetRotation function is returning this angle - " + GetRotation(GetNearest(collectibleList).transform.position));
        // If something detection in ray get new direction always, but this should be handled in Update <-
        Debug.Log("Stay Trigger - Currently there is/are " + collectibleList.Count + " collectibles within reach");

        // collectibleList.Add(other.gameObject); -> do not increment this on stay
        // Go to collectible here
        // if we detect something go to raycast?
        /*
        if (GetRotation(GetNearest(collectibleList).transform.position) + 10f >= transform.rotation.y && GetRotation(GetNearest(collectibleList).transform.position) - 10f <= transform.rotation.y)
        {
            Debug.Log("I am in if match");
            // do nothing keep going in same direction

        }
        else
        {
            transform.Rotate(0, GetRotation(GetNearest(collectibleList).transform.position), 0);
            Debug.Log("I am updating angle");
             // lerp for smoothness?
        }
        */
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.gameObject.tag == "Collectible" && collectibleList.Contains(other.gameObject))
        {
            collectibleList.Remove(other.gameObject);
            Debug.Log("Exit Trigger - Currently there is/are " + collectibleList.Count + " collectibles within reach");
        } // ^ should this be really here? Answer - Yes, otherwise we will have a problem with for loop in ontriggerenter

    }

    

    private GameObject GetNearest(List<GameObject> collectibles)
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

    /*

    private void OnTriggerStay(Collider other)
    {
    // Same code applies here. On Trigger Enter rotate towards closest collectible, and on stay keep rotating towards that.
    // create array to store all collectible objects within sphere, calculate distance to each one go to nearest
        if (other.gameObject.tag == "Collectible")
        {
        // add to list here, iterate through the list first to ensure that the object has not already been added.

        Debug.Log("RBS Angle before calling method without 360 " + (transform.rotation.eulerAngles));
        Debug.Log("RBS Angle before calling method " + (transform.rotation.eulerAngles.y - 360f));
        // check instance ID?
        if (Mathf.Approximately(GetRotation(GetNearest(collectibleList).transform.position), transform.rotation.eulerAngles.y - 360f) ||
           (GetRotation(GetNearest(collectibleList).transform.position) + 10f >= (transform.rotation.eulerAngles.y - 360f) && GetRotation(GetNearest(collectibleList).transform.position) - 10f <= (transform.rotation.eulerAngles.y - 360f)))
        {
            // do nothing
            Debug.Log("Do nothing!");
        }
        else
        {
            // change angle
            Debug.Log("Change Angle");
            transform.Rotate(0, GetRotation(GetNearest(collectibleList).transform.position), 0);
        }
        Debug.Log("RBS Angle is " + (transform.rotation.eulerAngles.y - 360f ));
        Debug.Log("GetNearest is returning this position -" + GetNearest(collectibleList).transform.position);
        Debug.Log("The GetRotation function is returning this angle - " + GetRotation(GetNearest(collectibleList).transform.position));
        // If something detection in ray get new direction always, but this should be handled in Update <-
        Debug.Log("Stay Trigger - Currently there is/are " + collectibleList.Count + " collectibles within reach");

        // collectibleList.Add(other.gameObject); -> do not increment this on stay
        // Go to collectible here
        // if we detect something go to raycast?
        /*
        if (GetRotation(GetNearest(collectibleList).transform.position) + 10f >= transform.rotation.y && GetRotation(GetNearest(collectibleList).transform.position) - 10f <= transform.rotation.y)
        {
            Debug.Log("I am in if match");
            // do nothing keep going in same direction

        }
        else
        {
            transform.Rotate(0, GetRotation(GetNearest(collectibleList).transform.position), 0);
            Debug.Log("I am updating angle");
             // lerp for smoothness?
        }



    }
    else
    {
        // think about putting this in ontriggerstay as well
        // do nothing, this is where it should go back and handle from update
    }
    }
    */
    private float GetRotation(Vector3 collectiblePosition)
    {
        // Calculate difference between cube and collectible
        Vector3 difference = collectiblePosition - transform.position;
        float rotationY = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;
        return rotationY;
        // Returns the angle of rotation for cube
    }


}
