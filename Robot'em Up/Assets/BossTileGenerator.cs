using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTileGenerator : MonoBehaviour
{
	public float tileSize;
	public GameObject tilePrefab;
	BoxCollider col;

	private void Awake ()
	{
		col = GetComponent<BoxCollider>();
		GenerateTiles();
	}
	void GenerateTiles()
	{
		float totalSizeX = col.bounds.size.x;
		float totalSizeY = col.bounds.size.z;
		int tileXAmount = Mathf.RoundToInt(totalSizeX / tileSize);
		int tileYAmount = Mathf.RoundToInt(totalSizeY / tileSize);

		Debug.Log(tileXAmount + " " + tileYAmount);
		for (int x = 0; x < tileXAmount; x++)
		{
			for (int y = 0; y < tileYAmount; y++)
			{
				GameObject newTile = Instantiate(tilePrefab);
				newTile.transform.position = transform.position + new Vector3(-tileSize * (tileXAmount / 2) + (x * tileSize) + (0.5f * tileSize), 0, -tileSize * (tileYAmount / 2f) + (y * tileSize) + (0.5f * tileSize));
				newTile.transform.localScale = Vector3.one * tileSize;
				newTile.layer = gameObject.layer;
				newTile.tag = gameObject.tag;
			}
		}
		Destroy(this.gameObject);
	}
}
