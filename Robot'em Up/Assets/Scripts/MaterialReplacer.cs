using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialReplacer : MonoBehaviour
{
	Renderer newRenderer;
	Material[] previousMaterials;
	public void ReplaceMaterial ( Material _newMaterial)
	{
		newRenderer = GetComponent<Renderer>();
		previousMaterials = newRenderer.materials;
		newRenderer.material = _newMaterial;
	}

	private void OnDestroy ()
	{
		if (newRenderer != null)
		{
			newRenderer.materials = previousMaterials;
		}
	}
}