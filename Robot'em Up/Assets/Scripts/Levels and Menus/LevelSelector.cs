using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
	public Font buttonFont;
	public List<Button> buttons;
	public List<string> scenesToHide;
	private void Awake ()
	{
		GenerateSceneList();
	}
	void GenerateSceneList()
	{
		for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
		{
			bool skip = false;
			foreach (string s in scenesToHide)
			{
				if (GameManager.GetSceneIndexFromName(s) == i)
				{
					skip = true;
				}
			}
			if (skip) { continue; }
			int x = i;
			GameObject i_newButton = new GameObject();
			i_newButton.name = "Button[" + SceneManager.GetSceneByBuildIndex(x).name + "]";
			Image i_image = i_newButton.AddComponent<Image>();
			i_image.sprite = Resources.Load<Sprite>("Menu/default_button");
			RectTransform i_buttonTransform = i_newButton.GetComponent<RectTransform>();
			i_buttonTransform.sizeDelta = new Vector2(200, 50);

			i_newButton.AddComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(x)) ;
			i_newButton.transform.SetParent(transform.Find("Viewport").transform.Find("Content"));

			GameObject i_buttonText = new GameObject();
			i_buttonText.transform.SetParent(i_newButton.transform);
			Text i_newText = i_buttonText.AddComponent<Text>();
			i_newText.alignment = TextAnchor.MiddleCenter;
			i_newText.rectTransform.sizeDelta = new Vector2(i_buttonTransform.sizeDelta.x*0.7f, i_buttonTransform.sizeDelta.y * 0.5f);
			i_newText.text = GetSceneNameFromBuildIndex(x);
			i_newText.resizeTextMinSize = 1;
			i_newText.resizeTextForBestFit = true;
			i_newText.font = buttonFont;

			buttons.Add(i_newButton.GetComponent<Button>());
		}
	}

	public static string GetSceneNameFromBuildIndex ( int _index )
	{
		string scenePath = SceneUtility.GetScenePathByBuildIndex(_index);
		string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

		return sceneName;
	}
}
