using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempRotator : MonoBehaviour
{
	public float rotationSpeed = 10f;
    // Update is called once per frame
    Quaternion startRotation;

    private void Start()
    {
        startRotation = transform.localRotation;
    }

    void Update()
    {
		transform.localRotation = Quaternion.Euler(new Vector3(startRotation.eulerAngles.x, Time.time * rotationSpeed, startRotation.eulerAngles.z)); 
    }
}
