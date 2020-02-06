using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Console : MonoBehaviour
{
	public static Console instance;

	private void Awake ()
	{
		instance = this;
	}

	public static void PrintMessage(string _message)
	{
		if (instance == null) { Debug.LogWarning("Can't print: Console isn't instantied yet"); return; }
		GameObject newMessage = Instantiate(Resources.Load<GameObject>("ConsoleResource/Text"));
		newMessage.transform.SetParent(instance.transform);
		newMessage.transform.localScale = Vector3.one;
		newMessage.GetComponentInChildren<Text>().text = _message;
		Destroy(newMessage.gameObject, 20);
	}
}
