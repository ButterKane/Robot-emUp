using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialReplacer : MonoBehaviour
{
	Renderer renderer;
	Material[] previousMaterials;
	public void ReplaceMaterial ( Material _newMaterial)
	{
		renderer = GetComponent<Renderer>();
		previousMaterials = renderer.materials;
		renderer.material = _newMaterial;
	}

	private void OnDestroy ()
	{
		if (renderer != null)
		{
			renderer.materials = previousMaterials;
		}
	}
}