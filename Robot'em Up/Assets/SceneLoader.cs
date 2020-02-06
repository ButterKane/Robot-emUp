using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	private int buildIndex;

	private void Awake ()
	{
		int countLoaded = SceneManager.sceneCount;
		Scene[] loadedScenes = new Scene[countLoaded];

		for (int i = 0; i < countLoaded; i++)
		{
			loadedScenes[i] = SceneManager.GetSceneAt(i);
		}
		buildIndex = loadedScenes[countLoaded-1].buildIndex;
		if (GameManager.GetSceneNameFromIndex(buildIndex) == "MainSceneTemplate")
		{
			buildIndex = loadedScenes[countLoaded - 2].buildIndex;
		}
	}

	public void LoadNextLevel()
	{
		for (int i = buildIndex + 1; i < buildIndex + 3; i++)
		{
			if (!SceneManager.GetSceneByBuildIndex(i).isLoaded)
			{
				SceneManager.LoadSceneAsync(i, LoadSceneMode.Additive);
			}
		}
		if (SceneManager.GetSceneByBuildIndex(buildIndex-1).isLoaded)
		{
			SceneManager.UnloadSceneAsync(buildIndex-1, UnloadSceneOptions.None);
		}
	}

	public void LoadPreviousLevel()
	{
		for (int i = buildIndex - 1; i > buildIndex - 3; i--)
		{
			if (i <= 0) { continue; }
			if (!SceneManager.GetSceneByBuildIndex(i).isLoaded)
			{
				SceneManager.LoadSceneAsync(i, LoadSceneMode.Additive);
			}
		}
		if (SceneManager.GetSceneByBuildIndex(buildIndex + 2).isLoaded)
		{
			SceneManager.UnloadSceneAsync(buildIndex + 2, UnloadSceneOptions.None);
		}
	}
}
