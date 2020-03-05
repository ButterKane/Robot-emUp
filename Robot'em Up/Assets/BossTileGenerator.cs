using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossTileGenerator : MonoBehaviour
{
	public static BossTileGenerator instance;
	public float tileSize;
	public GameObject tilePrefab;
	public NavMeshSurface nms;
	BoxCollider col;

	private void Awake ()
	{
		col = GetComponent<BoxCollider>();
		instance = this;
	}
	public List<BossTile> GenerateTiles()
	{
		List<BossTile> tiles = new List<BossTile>();
		float totalSizeX = col.bounds.size.x;
		float totalSizeY = col.bounds.size.z;
		int tileXAmount = Mathf.RoundToInt(totalSizeX / tileSize);
		int tileYAmount = Mathf.RoundToInt(totalSizeY / tileSize);

		for (int y = 0; y < tileYAmount; y++)
		{
			for (int x = 0; x < tileXAmount; x++)
			{
				GameObject newTile = Instantiate(tilePrefab);
				newTile.transform.position = transform.position + new Vector3(-tileSize * (tileXAmount / 2) + (x * tileSize) + (0.5f * tileSize), 0, -tileSize * (tileYAmount / 2f) + (y * tileSize) + (0.5f * tileSize));
				newTile.transform.localScale = Vector3.one * tileSize;
				newTile.layer = gameObject.layer;
				newTile.tag = gameObject.tag;
				foreach (Transform t in newTile.transform) {
					t.gameObject.layer = gameObject.layer;
					t.gameObject.tag = gameObject.tag;
				}
				tiles.Add(newTile.GetComponent<BossTile>());
			}
		}
		col.enabled = false;
		GetComponent<MeshRenderer>().enabled = false;
		nms = GetComponent<NavMeshSurface>();
		nms.layerMask = LayerMask.GetMask("Environment");
		nms.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
		nms.BuildNavMesh();
		return tiles;
	}
}
