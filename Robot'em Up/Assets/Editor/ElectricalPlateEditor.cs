using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(ElectricalPlateResizer))]
public class ElectricalPlateEditor : Editor
{
	private ElectricalPlateResizer plate;

	private void OnEnable ()
	{
		plate = (ElectricalPlateResizer)target;
		if (!Application.isPlaying && !plate.setupDone)
		{
			SetupPlate();
			ReshapeElectricalPlate(new Vector2(plate.width, plate.length));
		}

	}
	public override void OnInspectorGUI ()
	{
		EditorGUI.BeginChangeCheck();
		SerializedProperty m_width = serializedObject.FindProperty("width");
		EditorGUILayout.PropertyField(m_width);

		SerializedProperty m_length = serializedObject.FindProperty("length");
		EditorGUILayout.PropertyField(m_length);
		serializedObject.ApplyModifiedProperties();

		if (EditorGUI.EndChangeCheck())
		{
			ReshapeElectricalPlate(new Vector2(plate.width, plate.length));
		}
		if (GUILayout.Button("Force resize"))
		{
			SetupPlate();
			ReshapeElectricalPlate(new Vector2(plate.width, plate.length));
		}
		EditorUtility.SetDirty(plate);
		serializedObject.ApplyModifiedProperties();
	}

	public void SetupPlate()
	{
		//Unpack prefab
		if (PrefabUtility.GetCorrespondingObjectFromSource(plate.gameObject) != null)
		{
			PrefabUtility.UnpackPrefabInstance(plate.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
		}

		//Remove old FX and old visuals
		Transform oldVisuals = plate.transform.Find("SM_ElectricPlates");
		if (oldVisuals != null)
		{
			DestroyImmediate(oldVisuals.gameObject);
		}

		Transform oldFX = plate.transform.Find("FX_ElectricPlate");
		if (oldFX != null)
		{
			DestroyImmediate(oldFX.gameObject);
		}

		foreach (Transform t in plate.transform)
		{
			DestroyImmediate(t.gameObject);
		}


		//Set correct size
		plate.transform.localScale = Vector3.one;
		plate.setupDone = true;

		EditorUtility.SetDirty(plate);
	}
	public void ReshapeElectricalPlate(Vector2 _size)
	{
		float plateDimension = 6.6f;
		Transform plateHolder = plate.transform.Find("PlateHolder");
		if (plateHolder != null)
		{
			DestroyImmediate(plateHolder.gameObject);
		}

		GameObject newHolder = new GameObject();
		newHolder.name = "PlateHolder";
		newHolder.transform.SetParent(plate.transform);
		newHolder.transform.localPosition = Vector3.zero;
		newHolder.transform.localScale = Vector3.one;
		Vector2 plateSize = _size;

		Vector2 finalSize = new Vector2(plateSize.x * plateDimension, plateSize.y * plateDimension);

		//Generates plates
		for (int x = 0; x < plateSize.x; x++)
		{
			for (int y = 0; y < plateSize.y; y++)
			{
				GameObject newPart = default;
				if (plateSize.x == 1)
				{
					newPart = (GameObject)PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("PuzzleResource/ElectricalPlate/PlatePart_DoubleSide"));
				} else if (x == 0 || x == plateSize.x-1)
				{
					newPart = (GameObject)PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("PuzzleResource/ElectricalPlate/PlatePart_Side"));
					if (x == plateSize.x -1)
					{
						newPart.transform.localRotation = Quaternion.Euler(new Vector3(0, -180, 0));
					}
				} else
				{
					newPart = (GameObject)PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("PuzzleResource/ElectricalPlate/PlatePart_Center"));
				}
				newPart.transform.SetParent(newHolder.transform);
				newPart.transform.localScale = Vector3.one;
				newPart.transform.localPosition = new Vector3(y * plateDimension - (finalSize.y / 2f - (plateDimension / 2f)), -0.4f, x * plateDimension - (finalSize.x / 2f - (plateDimension / 2f)));
			}
		}

		//Generates box collider
		BoxCollider bc = plate.GetComponent<BoxCollider>();
		if (bc == null)
		{
			bc = plate.gameObject.AddComponent<BoxCollider>();
		}
		bc.size = new Vector3(finalSize.x, 3f, finalSize.y);
		bc.center = new Vector3(0f, 1f, 0f);
		bc.isTrigger = true;

		//Update plate FX list
		PuzzleEletricPlate plateScript = plate.GetComponent<PuzzleEletricPlate>();
		List<GameObject> plateFXParent = new List<GameObject>();
		List<ParticleSystem> FXTotalList = new List<ParticleSystem>();
		if (plateScript != null)
		{
			foreach (Transform t in newHolder.transform)
			{
				plateFXParent.Add(t.Find("FX_ElectricPlate").gameObject);
			}			
		}
		foreach (GameObject g in plateFXParent)
		{
			FXTotalList.Add(g.GetComponent<ParticleSystem>());
			foreach (Transform t in g.transform)
			{
				FXTotalList.Add(t.GetComponent<ParticleSystem>());
			}
		}
		plateScript.FXS = FXTotalList;
		plateScript.indictatorLightsList = new List<Light>();
		EditorUtility.SetDirty(plateScript);
	}
}