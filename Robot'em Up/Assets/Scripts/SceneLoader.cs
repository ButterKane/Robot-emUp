using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
[CreateAssetMenu(fileName = "SceneLoader", menuName = "GlobalDatas/SceneLoader", order = 1)]
public class SceneLoader : ScriptableObject
{
	[SerializeField]
	IntSceneDictionary m_sceneIndexes;
	public IDictionary<int, SceneAsset> IntSceneDictionary
	{
		get { return m_sceneIndexes; }
		set { m_sceneIndexes.CopyFrom(value); }
	}

	void Reset ()
	{
		// access by property
		IntSceneDictionary = new Dictionary<int, SceneAsset>() {};
	}
	public string GetSceneByIndex(int index)
	{
		SceneAsset sceneFound;
		if (m_sceneIndexes.TryGetValue(index, out sceneFound))
		{
			if (sceneFound != null)
			{
				return sceneFound.name;
			} else
			{
				return "";
			}
		} else
		{
			return "";
		}
	}
}
#endif