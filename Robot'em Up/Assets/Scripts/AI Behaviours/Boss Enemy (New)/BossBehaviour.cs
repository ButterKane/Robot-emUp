using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossBehaviour : MonoBehaviour
{
	public Transform topPart;
	public float topPartRotationSpeed;
	public GameObject healthBarPrefab;
	private HealthBar healthBar1;
	private HealthBar healthBar2;


	private NavMeshAgent navMesh;

	private void Awake ()
	{
		navMesh = GetComponent<NavMeshAgent>();
		GenerateHealthBar();
	}
	private void Update ()
	{
		navMesh.SetDestination(GameManager.playerOne.transform.position);
		Vector3 lookPosition = GameManager.playerOne.transform.position;
		lookPosition.y = topPart.transform.position.y;
		Quaternion wantedRotation = Quaternion.LookRotation(lookPosition - topPart.transform.position);
		topPart.transform.rotation = Quaternion.Lerp(topPart.transform.rotation, wantedRotation, Time.deltaTime * topPartRotationSpeed);
	}

	public void Damage(float _amount)
	{

	}

	public void GenerateHealthBar()
	{

	}
}
