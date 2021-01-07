using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelector : MonoBehaviour
{
	public TMP_FontAsset buttonFont;
    public string[] levelNames;
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
			i_newButton.transform.localScale = Vector3.one * 0.8f;
			Scene foundScene = SceneManager.GetSceneByBuildIndex(x);
			i_newButton.name = "Button[" + foundScene.name + "]";
			Image i_image = i_newButton.AddComponent<Image>();
			i_image.color = new Color(0, 0, 0, 0);
			//i_image.sprite = Resources.Load<Sprite>("Menu/default_button02");
			RectTransform i_buttonTransform = i_newButton.GetComponent<RectTransform>();
			i_buttonTransform.sizeDelta = new Vector2(200, 30);

			i_newButton.AddComponent<Button>().onClick.AddListener(() => LoadingScreen.StartLoadingScreen(new UnityAction(() => SceneManager.LoadScene(x))));
			i_newButton.transform.SetParent(transform.Find("Viewport").transform.Find("Content"));
			//i_newButton.transform.localPosition = new Vector3(0f, -i * 50, 0);

			GameObject i_buttonText = new GameObject();
			i_buttonText.transform.SetParent(i_newButton.transform);
			RectTransform rt = i_buttonText.AddComponent<RectTransform>();
			rt.sizeDelta = new Vector2(2000, 25);
			TextMeshProUGUI i_newText = i_buttonText.AddComponent<TextMeshProUGUI>();
			i_newText.alignment = TextAlignmentOptions.MidlineLeft;
			//i_newText.rectTransform.sizeDelta = new Vector2(i_buttonTransform.sizeDelta.x*0.7f, i_buttonTransform.sizeDelta.y * 0.5f);
			i_newText.text = levelNames[x].ToUpper();
			i_newText.fontSize = 25;
			i_newText.font = buttonFont;
			i_newText.enableAutoSizing = true;
			i_newText.fontSizeMin = 5;
			i_newText.fontSizeMax = 20;
			rt.pivot = new Vector2(0.5f, 0.5f);
			i_newText.transform.localPosition = Vector3.zero;

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
