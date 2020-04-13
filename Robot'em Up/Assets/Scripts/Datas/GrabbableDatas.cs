using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GrabbableDatas", menuName = "GameDatas/GrabbableDatas")]
public class GrabbableDatas : ScriptableObject
{
	public Material editorLineRendererMaterial;
	public float editorLineRendererWidth;

	public Material ingameLineRendererMaterial;
	public float ingameLineRendererWidth;

	public GameObject grabAvailableUIPreview;


	public static GrabbableDatas GetDatas()
	{
		return Resources.Load<GrabbableDatas>("GrabbableDatas");
	}
}
