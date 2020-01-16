using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ParticleAutoDestroyer : MonoBehaviour
{
	private ParticleSystem ps;


	public void Start ()
	{
		ps = GetComponent<ParticleSystem>();
	}

	public void Update ()
	{
		if (ps)
		{
			if (!ps.IsAlive())
			{
				Destroy(gameObject);
			}
		}
	}
}
