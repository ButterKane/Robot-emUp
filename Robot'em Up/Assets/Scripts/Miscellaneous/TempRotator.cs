using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempRotator : MonoBehaviour
{
	public float rotationSpeed = 10f;
    // Update is called once per frame
    void Update()
    {
		transform.rotation = Quaternion.Euler(new Vector3(0, Time.time * rotationSpeed, 0)); 
    }
}
