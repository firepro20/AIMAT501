using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject target;
    // Start is called before the first frame update
    public float speed = 4f;
    public float rotationSpeed = 100f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        float translation = Input.GetAxis("Vertical");
        float rotation = Input.GetAxis("Horizontal");

        transform.Translate(0, 0, translation * Time.deltaTime);

        transform.Rotate(0, rotation * Time.deltaTime, 0);

    }
}
