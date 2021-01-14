using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	private int buildIndex;
	private void Awake ()
	{
		/*
		int countLoaded = SceneManager.sceneCount;
		Scene[] loadedScenes = new Scene[countLoaded];

		for (int i = 0; i < countLoaded; i++)
		{
			loadedScenes[i] = SceneManager.GetSceneAt(i);
		}
		buildIndex = loadedScenes[countLoaded - 1].buildIndex;
		if (GameManager.GetSceneNameFromIndex(buildIndex) == "MainSceneTemplate")
		{
			buildIndex = loadedScenes[countLoaded - 2].buildIndex;
		}
		*/
		buildIndex = gameObject.scene.buildIndex;
	}

	public void LoadNextLevel ()
	{
		for (int i = buildIndex + 1; i < buildIndex + 3; i++)
		{
			if (!SceneManager.GetSceneByBuildIndex(i).isLoaded)
			{
				bool activateScene = true;
				if (i == buildIndex + 1) { activateScene = true; }
				StartCoroutine(LoadLevelAsynchronously_C(i, LoadSceneMode.Additive, activateScene));
			}
		}

		if (buildIndex - 1 >= 0 && SceneManager.GetSceneByBuildIndex(buildIndex - 1).isLoaded)
		{
			StartCoroutine(UnloadLevelAsynchronously_C(buildIndex - 1, UnloadSceneOptions.None));
		}
		GameManager.ChangeCurrentZone(GameManager.GetSceneNameFromIndex(buildIndex + 1));
	}

	public void LoadPreviousLevel ()
	{
		for (int i = buildIndex - 1; i > buildIndex - 3; i--)
		{
			if (i <= 0) { continue; }
			if (!SceneManager.GetSceneByBuildIndex(i).isLoaded)
			{
				bool activateScene = true;
				if (i == buildIndex - 1) { activateScene = true; }
				StartCoroutine(LoadLevelAsynchronously_C(i, LoadSceneMode.Additive, activateScene));
			}
		}
		if (SceneManager.GetSceneByBuildIndex(buildIndex + 2).isLoaded)
		{
			StartCoroutine(UnloadLevelAsynchronously_C(buildIndex + 2, UnloadSceneOptions.None));
		}
		GameManager.ChangeCurrentZone(GameManager.GetSceneNameFromIndex(buildIndex - 1));
	}

	IEnumerator LoadLevelAsynchronously_C ( int _buildIndex, LoadSceneMode _mode, bool _setActiveScene = false )
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_buildIndex, _mode);
		asyncLoad.allowSceneActivation = false;
		while (asyncLoad.progress < 0.9f)
		{
			yield return null;
		}
		asyncLoad.allowSceneActivation = true;
		while (asyncLoad.progress < 1.0f)
		{
			yield return null;
		}
		if (_setActiveScene)
		{
			SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(_buildIndex));
		}
	}

	IEnumerator UnloadLevelAsynchronously_C ( int _buildIndex, UnloadSceneOptions _mode )
	{
		AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(_buildIndex, _mode);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}
}
