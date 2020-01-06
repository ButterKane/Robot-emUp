using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
	public Font buttonFont;
	public List<Button> buttons;
	private void Awake ()
	{
		GenerateSceneList();
	}
	void GenerateSceneList()
	{
		for (int i = 1; i <= SceneManager.sceneCountInBuildSettings-1; i++)
		{
			int x = i;
			GameObject internal_newButton = new GameObject();
			internal_newButton.name = "Button[" + SceneManager.GetSceneByBuildIndex(x).name + "]";
			Image internal_image = internal_newButton.AddComponent<Image>();
			internal_image.sprite = Resources.Load<Sprite>("Menu/default_button");
			RectTransform internal_buttonTransform = internal_newButton.GetComponent<RectTransform>();
			internal_buttonTransform.sizeDelta = new Vector2(200, 50);

			internal_newButton.AddComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(x)) ;
			internal_newButton.transform.SetParent(transform.Find("Viewport").transform.Find("Content"));

			GameObject internal_buttonText = new GameObject();
			internal_buttonText.transform.SetParent(internal_newButton.transform);
			Text internal_newText = internal_buttonText.AddComponent<Text>();
			internal_newText.alignment = TextAnchor.MiddleCenter;
			internal_newText.rectTransform.sizeDelta = new Vector2(internal_buttonTransform.sizeDelta.x*0.7f, internal_buttonTransform.sizeDelta.y * 0.5f);
			internal_newText.text = GetSceneNameFromBuildIndex(x);
			internal_newText.resizeTextMinSize = 1;
			internal_newText.resizeTextForBestFit = true;
			internal_newText.font = buttonFont;

			buttons.Add(internal_newButton.GetComponent<Button>());
		}
	}

	public static string GetSceneNameFromBuildIndex ( int _index )
	{
		string scenePath = SceneUtility.GetScenePathByBuildIndex(_index);
		string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

		return sceneName;
	}
}
