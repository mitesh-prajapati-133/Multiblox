using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loading : MonoBehaviour
{
   public float speed = 200f; // rotation speed

    void Update()
    {
        // Only spin if active
        if (gameObject.activeSelf)
        {
            transform.Rotate(Vector3.forward * -speed * Time.deltaTime);
        }
    }

}
