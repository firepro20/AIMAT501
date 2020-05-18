using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance { get { return instance; } }

    public int agentStartHealth = 100;
    public int lifeDeductionRate = 1;

    public GameObject academy;

    // UI
    public bool toggleRBS;
    public bool toggleRL;

    public GameObject agentRBS;
    public Image RBSHealth;
    public GameObject agentRL;
    public Image RLHealth;
    public GameObject RBSTime;
    public GameObject RLTime;
    [HideInInspector]
    public bool gameRunning = false;
    public Stopwatch stopWatchRBS;
    public Stopwatch stopWatchRL;

    private float agentRBSHealth;
    private float agentRLHealth;
    private float deductionRate;
    private List<GameObject> totalHealthItems;
    private TimeSpan tsRBS;
    private TimeSpan tsRL;
    private string rbsTime;
    private string rlTime;
    

    private void Awake()
    {
        if (instance == null) // if there is nothing stored in here at the start of the game, then set myself to be contained within this instance were code is running in
        {
            instance = this; // this cookie
        }
        else
        {
            DestroyImmediate(gameObject); // keep first and original instance at all cost
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameRunning = true;
        CheckAgentToggle();
        

        rbsTime = RBSTime.GetComponent<Text>().ToString();
        rlTime = RLTime.GetComponent<Text>().ToString();
        /* Complicating life for no reason, supply this through Inspector

        // store gameObjects as well for easier modification
        foreach (GameObject child in stopwatchUI.GetComponentsInChildren<GameObject>())
        {
            if (child.name.Contains("RBS"))
            {
                String text = rbsTime.gameObject.AddComponent<Text>();

                //rbsTime = child.GetComponent<Text>().ToString(); // store starting data
            }
            if (child.name.Contains("RL"))
            {
                //rlTime = text.ToString(); // store starting data
            }
        }

        */
    }

    // Update is called once per frame
    void Update()
    {
        //UnityEngine.Debug.LogWarning("Current RL Health is " + agentRLHealth);
        /*

        // Switch between agents
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            toggleRBS = true;
            toggleRL = false;
            CheckAgentToggle();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            toggleRL = true;
            toggleRBS = false;
            CheckAgentToggle();
        }

        */

        // get health and image reduce by 10 each 2 seconds. 
        if (!(agentRBSHealth <= 0) && !(agentRLHealth <= 0)) { 
            TakeHealth(deductionRate);
            RBSHealth.fillAmount = agentRBSHealth / agentStartHealth; // this might cause a problem when we have two separate agents, maybe decide to switch between them
            RLHealth.fillAmount = agentRLHealth / agentStartHealth;
        }
        if (agentRBSHealth <= 0) // might cause weird bug
        { // if rbs health or rl health stop and do stuff accordingly for both

            // stop stopwatch
            
            // present time
        }
        if (toggleRBS && !(GameObject.FindGameObjectWithTag("Collectible")))
        {
            stopWatchRBS.Stop();
        }
        if (agentRLHealth <= 0 || !GameObject.FindGameObjectWithTag("Collectible")) // || !GameObject.FindGameObjectWithTag("Collectible")
        {
            // stop stopwatch
            // comment for training
            Time.timeScale = 0;
            stopWatchRL.Stop();
            if (Input.GetKeyDown(KeyCode.Space))
                SceneManager.LoadScene("SampleScene");
            //
            //present time
        }
        if (agentRBSHealth >= agentStartHealth)
        {
            agentRBSHealth = agentStartHealth;
        }
        if (agentRLHealth >= agentStartHealth){
            agentRLHealth = agentStartHealth;
        }
        
        if (agentRBSHealth <= 0 || !GameObject.FindGameObjectWithTag("Collectible")) // agentRLHealth <= 0 removed for MLAgents to handle this
        {
            //Time.timeScale = 0; // This is too powerful and was a cause of a major scene loading issue, time stopped and health was not being deducted
            gameRunning = false;
            Time.timeScale = 0;
            stopWatchRBS.Stop();
            if (GameObject.FindGameObjectWithTag("Collectible"))
            {
                totalHealthItems = new List<GameObject>(GameObject.FindGameObjectsWithTag("Collectible"));
                //UnityEngine.Debug.LogError("Collectibles Left - " + totalHealthItems.Count);
                return;
            }
            if(Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene("SampleScene");
            gameRunning = true;
            // DestroyImmediate(gameObject); // -> Causing issue, singleton is causing issue with reload.
        }
        
        
        
        // then work out take / give health for general use.
        tsRBS = stopWatchRBS.Elapsed;
        tsRL = stopWatchRL.Elapsed;

        UpdateStopwatch();
        // This information can be used to pop up the current elapsed time and represent final value when agent dies

        // Debugging tools

        //UnityEngine.Debug.Log("Elapsed Time for RBS - " + tsRBS);
        //UnityEngine.Debug.Log("Elapsed Time for RL - " + tsRL);
        //UnityEngine.Debug.Log("Agent RL Health - " + agentRLHealth);
        //UnityEngine.Debug.Log("Agent RBS Health - " + agentRBSHealth);
    }


    public void UpdateStopwatch()
    {
        if (toggleRBS && RBSTime != null)
        {
            RBSTime.GetComponent<Text>().text = rbsTime + "\n" + tsRBS.ToString(); // Remove garbage data
        }
        if (toggleRL && RLTime != null)
        {
            RLTime.GetComponent<Text>().text = rlTime + "\n" + tsRL.ToString(); // Remove garbage data
        }
    }

    public float GetRBSHealth()
    {
        return agentRBSHealth;
    }

    public float GetRLHealth()
    {
        return agentRLHealth;
    }

    public void TakeHealth(float seconds)
    {
        if (toggleRBS)
        {
            agentRBSHealth -= seconds * Time.deltaTime;
        }
        else
        {
            agentRLHealth -= seconds * Time.deltaTime;
        }
    }

    public void ReceiveDamage(GameObject agent, int healthToDeduct)
    {
        if (agent.GetComponent<RBSController>())
        {
            agentRBSHealth -= healthToDeduct;
        }
        else if (agent.GetComponent<RLController>())
        {
            agentRLHealth -= healthToDeduct;
        }
    }

    public void ReceiveHealth(GameObject agent, int healthToReceive)
    {
        if(agent.GetComponent<RBSController>())
        {
            agentRBSHealth += healthToReceive;
        }
        else if (agent.GetComponent<RLController>())
        {
            agentRLHealth += healthToReceive;
        }   
    }

    private void CheckAgentToggle()
    {
        if (toggleRBS == false)
        {
            agentRBS.SetActive(false);
            RBSHealth.GetComponentInParent<Text>().enabled = false;
            RBSHealth.enabled = false;
            RBSTime.SetActive(false);
        }
        if (toggleRL == false)
        {
            agentRL.SetActive(false);
            RLHealth.GetComponentInParent<Text>().enabled = false;
            RLHealth.enabled = false;
            RLTime.SetActive(false);
            academy.SetActive(false);
        }
        agentRBSHealth = agentRLHealth = agentStartHealth;
        deductionRate = lifeDeductionRate;
        stopWatchRBS = new Stopwatch();
        stopWatchRL = new Stopwatch();
        stopWatchRBS.Start();
        stopWatchRL.Start();
    }
}
