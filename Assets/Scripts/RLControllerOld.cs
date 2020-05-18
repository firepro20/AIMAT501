using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLControllerOld : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 100f;
    public int healthOnPickup = 10;
    public int healthOnHazard = 10;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, Input.GetAxis("Horizontal") * Time.deltaTime * rotationSpeed, 0);
        transform.Translate(0, 0, Input.GetAxis("Vertical") * Time.deltaTime * speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Collectible")
        {
            GameController.Instance.ReceiveHealth(gameObject, healthOnPickup);
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.tag == "Hazard")
        {
            GameController.Instance.ReceiveDamage(gameObject, healthOnHazard);
        }
        //UnityEngine.Debug.Log("RL collided with - " + collision.gameObject.tag); // continue from here
    }
}
