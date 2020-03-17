using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTile : MonoBehaviour
{
	public Transform visuals;

	private float currentElectricalPlateToggleCD;

	private PuzzleEletricPlate currentElectricalPlate;
	private BossSettings bossSettings;

	private void Awake ()
	{
		bossSettings = BossSettings.GetDatas();	
	}
	public void DestroyTile()
	{
		StartCoroutine(Destroy_Tile_C());
	}

	public void SpawnElectricalPlate()
	{
		if (currentElectricalPlate != null || visuals.GetComponent<Collider>().enabled == false) { return; }
		StartCoroutine(SpawnElectricalPlate_C());
	}

	public void DespawnElectricalPlate()
	{
		if (currentElectricalPlate == null) { return; }
		StartCoroutine(DespawnElectricalPlate_C());
	}

	private void Update ()
	{
		if (currentElectricalPlate != null)
		{
			if (currentElectricalPlateToggleCD <= 0)
			{
				ToggleElectricalPlateState();
			} else
			{
				currentElectricalPlateToggleCD -= Time.deltaTime;
			}
		}
	}

	void ToggleElectricalPlateState()
	{
		if (currentElectricalPlate.isActivated)
		{
			currentElectricalPlate.WhenDesactivate();
			currentElectricalPlateToggleCD = bossSettings.electricalPlateSettings.desactivationDuration;
		} else
		{
			currentElectricalPlate.WhenActivate();
			currentElectricalPlateToggleCD = bossSettings.electricalPlateSettings.activationDuration;
		}
	}

	IEnumerator SpawnElectricalPlate_C ()
	{
		GameObject electricalPlate = Instantiate(bossSettings.electricalPlateSettings.platePrefab, transform);
		currentElectricalPlate = electricalPlate.GetComponent<PuzzleEletricPlate>();
		currentElectricalPlate.WhenDesactivate();
		electricalPlate.transform.localScale = new Vector3(1f, 0.1f, 1f);
		Vector3 startPosition = transform.position + Vector3.down * 5f;
		Vector3 endPosition = transform.position + Vector3.down * -0.1f;
		for (float i = 0; i < 1f; i+=Time.deltaTime)
		{
			electricalPlate.transform.position = Vector3.Lerp(startPosition, endPosition, i / 1f);
			yield return null;
		}
		yield return new WaitForSeconds(bossSettings.electricalPlateSettings.duration);
		DespawnElectricalPlate();
	}

	IEnumerator DespawnElectricalPlate_C()
	{
		currentElectricalPlate.WhenDesactivate();
		Vector3 startPosition = transform.position + Vector3.down * -0.1f;
		Vector3 endPosition = transform.position + Vector3.down * 5f;
		for (float i = 0; i < 1f; i += Time.deltaTime)
		{
			currentElectricalPlate.transform.position = Vector3.Lerp(startPosition, endPosition, i / 1f);
			yield return null;
		}
		Destroy(currentElectricalPlate.gameObject);
	}

	IEnumerator Destroy_Tile_C ()
	{
		if (currentElectricalPlate != null) { Destroy(currentElectricalPlate.gameObject); }
		Vector3 startPosition = visuals.position;
		visuals.GetComponent<Collider>().enabled = false;
		yield return null;
		BossTileGenerator.instance.nms.BuildNavMesh();
		for (float i = 0; i < bossSettings.tilesSettings.fallDuration; i += Time.deltaTime)
		{
			visuals.position = Vector3.Lerp(startPosition, startPosition + Vector3.down * 20f, bossSettings.tilesSettings.fallSpeedCurve.Evaluate(i / bossSettings.tilesSettings.fallDuration));
			yield return null;
		}
		visuals.gameObject.SetActive(false);
		StartCoroutine(RespawnAfterDelay_C());
	}

	IEnumerator RespawnAfterDelay_C()
	{
		yield return new WaitForSeconds(bossSettings.tilesSettings.reapparitionDelay);
		Vector3 startPosition = visuals.position;
		visuals.GetComponent<Collider>().enabled = true;
		visuals.gameObject.SetActive(true);
		for (float i = 0; i < bossSettings.tilesSettings.fallDuration; i += Time.deltaTime)
		{
			visuals.position = Vector3.Lerp(startPosition, startPosition + Vector3.up * 20f, bossSettings.tilesSettings.fallSpeedCurve.Evaluate(i / bossSettings.tilesSettings.fallDuration));
			yield return null;
		}
	}
}
