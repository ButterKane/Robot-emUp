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
			GameObject newButton = new GameObject();
			newButton.name = "Button[" + SceneManager.GetSceneByBuildIndex(x).name + "]";
			Image image = newButton.AddComponent<Image>();
			image.sprite = Resources.Load<Sprite>("Menu/default_button");
			RectTransform buttonTransform = newButton.GetComponent<RectTransform>();
			buttonTransform.sizeDelta = new Vector2(200, 50);

			newButton.AddComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(x));
			newButton.transform.SetParent(transform.Find("Viewport").transform.Find("Content"));

			GameObject buttonText = new GameObject();
			buttonText.transform.SetParent(newButton.transform);
			Text newText = buttonText.AddComponent<Text>();
			newText.alignment = TextAnchor.MiddleCenter;
			newText.rectTransform.sizeDelta = new Vector2(buttonTransform.sizeDelta.x*0.7f, buttonTransform.sizeDelta.y * 0.5f);
			newText.text = GetSceneNameFromBuildIndex(x);
			newText.resizeTextMinSize = 1;
			newText.resizeTextForBestFit = true;
			newText.font = buttonFont;

			buttons.Add(newButton.GetComponent<Button>());
		}
	}

	public static string GetSceneNameFromBuildIndex ( int index )
	{
		string scenePath = SceneUtility.GetScenePathByBuildIndex(index);
		string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

		return sceneName;
	}
}
