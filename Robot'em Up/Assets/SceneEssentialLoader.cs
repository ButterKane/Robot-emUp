using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class SceneEssentialLoader : MonoBehaviour
{
	public bool alsoLoadNextScene;
	public Transform player1Position;
	public Transform player2Position;
	public Transform ballPosition;
	public Transform cameraPosition;
	public Light previewLight;
	public Camera previewCamera;
	public SceneLoader sceneLoader;
	private void Start ()
	{
		if (GameManager.i != null) { 
			if (sceneLoader != null)
			{
				sceneLoader.transform.SetParent(null);
			}
			Destroy(this.gameObject);
			return; 
		}
		StartCoroutine(ReplaceScene_C());
	}

	IEnumerator ReplaceScene_C()
	{
		SceneManager.LoadScene("MainSceneTemplate", LoadSceneMode.Additive);
		yield return null;
		PlayerController player1 = GameManager.playerOne;
		PlayerController player2 = GameManager.playerTwo;
		BallBehaviour ball = GameManager.ball;

		DontDestroyOnLoad(GameManager.i);
		DontDestroyOnLoad(player1);
		DontDestroyOnLoad(player2);
		DontDestroyOnLoad(ball);
		DontDestroyOnLoad(Camera.main);

		GameManager.DDOL.Add(player1.gameObject);
		GameManager.DDOL.Add(player2.gameObject);
		GameManager.DDOL.Add(ball.gameObject);
		GameManager.DDOL.Add(Camera.main.gameObject);
		GameManager.DDOL.Add(GameManager.i.gameObject);

		if (player1Position != null)
		{
			player1.transform.position = player1Position.position;
			Destroy(player1Position.gameObject);
		}

		if (player2Position != null)
		{
			player2.transform.position = player2Position.position;
			Destroy(player2Position.gameObject);
		}

		if (ballPosition != null)
		{
			ball.transform.position = ballPosition.position;
			Destroy(ballPosition.gameObject);
		}

		if (cameraPosition != null)
		{
			Camera.main.transform.position = cameraPosition.position;
			cameraPosition.name = "StartCamera";
			CinemachineVirtualCamera virtualCam = cameraPosition.GetComponent<CinemachineVirtualCamera>();
			if (virtualCam == null)
			{
				virtualCam = cameraPosition.gameObject.AddComponent<CinemachineVirtualCamera>();
			}
			cameraPosition.transform.SetParent(null);
			virtualCam.m_Priority = 10;
		}

		if (previewCamera != null)
			Destroy(previewCamera.gameObject);

		if (previewLight != null)
			Destroy(previewLight.gameObject);

		if (alsoLoadNextScene)
		{
			yield return new WaitForEndOfFrame();
			SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Additive);
		}

		if (sceneLoader != null)
			sceneLoader.transform.SetParent(null);

		Destroy(this.gameObject);
	}
}
