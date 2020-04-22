using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SceneReplacer : MonoBehaviour
{
	[System.Serializable]
	public class ReplaceInformation
	{
		public string objectToReplace;
		public Object replacingObject;
	}

	public List<ReplaceInformation> replaceInformations;

	[ContextMenu("Replace")]
	public void Replace()
	{
		List<GameObject> foundObjects = new List<GameObject>();
		foreach (GameObject foundObj in FindObjectsOfType<GameObject>())
		{
			foreach (ReplaceInformation ri in replaceInformations)
			{
				if (foundObj.name.Contains(ri.objectToReplace))
				{
					Debug.Log("[SceneReplacer] Replacing " + foundObj.name + " with " + ri.replacingObject.name);
					foundObjects.Add(foundObj);
					Object replacingObj = PrefabUtility.InstantiatePrefab(ri.replacingObject);
					GameObject newObj = (GameObject)replacingObj;
					newObj.transform.parent = foundObj.transform.parent;
					newObj.transform.position = foundObj.transform.position;
					newObj.transform.name = foundObj.name;
					newObj.transform.localRotation = foundObj.transform.localRotation;
					newObj.transform.localScale = foundObj.transform.localScale;
				}
			}
		}
		foreach (GameObject go in foundObjects)
		{
			DestroyImmediate(go);
		}
	}

}
