using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Moving camera after movement has completed in other Update calls, ensures this is the last thing to happen
    private void LateUpdate()
    {
        if (GameController.Instance.agentRBS == null)
        {
            // do nothing
        }
        else if (GameController.Instance.agentRBS.gameObject.activeSelf)
        {
            transform.position = new Vector3(GameController.Instance.agentRBS.transform.position.x,
                                             transform.position.y,
                                             GameController.Instance.agentRBS.transform.position.z);
        }

        

        if (GameController.Instance.agentRL == null)
        {
            // do nothing
        }
        else if (GameController.Instance.agentRL.gameObject.activeSelf)
        {
            transform.position = new Vector3(GameController.Instance.agentRL.transform.position.x,
                                             transform.position.y,
                                             GameController.Instance.agentRL.transform.position.z);
        }

    }
}
