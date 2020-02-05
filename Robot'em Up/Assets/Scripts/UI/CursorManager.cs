using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
	public GameObject ballPointerPrefab;
	private static GameObject ballPointer;
	private static Transform ballPointerParent;

    // Start is called before the first frame update
    void Start()
    {
		ballPointer = Instantiate(ballPointerPrefab);
		DontDestroyOnLoad(ballPointer);
		ballPointer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
		if (ballPointer && ballPointerParent)
		{
			ballPointer.transform.position = ballPointerParent.transform.position;
		}
    }

	public static void ShowBallPointer(bool state)
	{
		switch (state)
		{
			case true:
				ballPointer.SetActive(true);
				break;
			case false:
				ballPointer.SetActive(false);
				break;
		}
	}

	public static void SetBallPointerParent(Transform _newParent)
	{
		if (_newParent == null) {
			ShowBallPointer(false);
			ballPointerParent = null;
		} else
		{
			ShowBallPointer(true);
			ballPointerParent = _newParent;
		}
	}
}
