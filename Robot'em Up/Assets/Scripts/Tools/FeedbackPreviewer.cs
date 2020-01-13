using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XInputDotNetPure;
using UnityEngine.SceneManagement;
using System.Reflection;

public class FeedbackPreviewer : EditorWindow
{
	Camera camera;
	RenderTexture renderTexture;
	private ParticleSystem fxPs;
	public static FeedbackPreviewer instance;

	public List<GameObject> instantiatedObjects;

	[MenuItem("Example/Feedback viewer")]
	static void Init ()
	{
		EditorWindow editorWindow = GetWindow(typeof(FeedbackPreviewer));
		editorWindow.autoRepaintOnSceneChange = true;
		editorWindow.Show();
	}

	public void Awake ()
	{
		instantiatedObjects = new List<GameObject>();
		renderTexture = new RenderTexture((int)position.width,
			(int)position.height,
			(int)RenderTextureFormat.ARGB32);
	}

	public void OnEnable ()
	{
		instance = this;
		//Disable all objects
		List<GameObject> rootObjects = new List<GameObject>();
		Scene scene = SceneManager.GetActiveScene();
		scene.GetRootGameObjects(rootObjects);
		for (int i = 0; i < rootObjects.Count; ++i)
		{
			GameObject gameObject = rootObjects[i];
			gameObject.SetActive(false);
			gameObject.hideFlags = HideFlags.HideInHierarchy;
		}
		//Generate camera
		GameObject cameraObject = new GameObject();
		cameraObject.name = "PreviewCamera";
		instantiatedObjects.Add(cameraObject);
		camera = cameraObject.AddComponent<Camera>();
		cameraObject.transform.position = new Vector3(0,0,-8);

		//Generate preview object
		GameObject previewObject = Instantiate(Resources.Load<GameObject>("FeedbackToolResources/PreviewMesh"));
		previewObject.transform.position = new Vector3(0,-1,0);
		instantiatedObjects.Add(previewObject);

		//Generate preview panel
		GameObject previewPanel = Instantiate(Resources.Load<GameObject>("FeedbackToolResources/PreviewPanel"));
		previewPanel.transform.position = new Vector3(0, 0, 4);
		instantiatedObjects.Add(previewPanel);
	}

	private void OnDestroy ()
	{
		foreach (GameObject obj in instantiatedObjects)
		{
			DestroyImmediate(obj);
		}
		List<GameObject> rootObjects = new List<GameObject>();
		Scene scene = SceneManager.GetActiveScene();
		scene.GetRootGameObjects(rootObjects);
		for (int i = 0; i < rootObjects.Count; ++i)
		{
			GameObject gameObject = rootObjects[i];
			gameObject.SetActive(true);
			gameObject.hideFlags = HideFlags.None;
		}
		instance = null;
	}


	public void Update ()
	{
		if (camera != null)
		{
			camera.targetTexture = renderTexture;
			camera.Render();
			camera.targetTexture = null;
		}
		if (renderTexture.width != position.width ||
			renderTexture.height != position.height)
			renderTexture = new RenderTexture((int)position.width,
				(int)position.height,
				(int)RenderTextureFormat.ARGB32);

		if (fxPs != null)
		{
			fxPs.Simulate(Time.deltaTime, true, false);
		}
		Repaint();
	}

	void OnGUI ()
	{
		GUI.DrawTexture(new Rect(0.0f, 0.0f, position.width, position.height), renderTexture);
	}

	public void PreviewFeedback(FeedbackData _data )
	{
		if (_data.vibrationDataInited)
		{
			VibrationManager.Vibrate(PlayerIndex.One, _data.vibrationData.duration, _data.vibrationData.force);
			VibrationManager.Vibrate(PlayerIndex.Two, _data.vibrationData.duration, _data.vibrationData.force);
		}
		if (_data.soundData.soundName != "" && _data.soundDataInited)
		{
			SoundManager.PlaySoundInEditor(_data.soundData.soundName);
		}
		if (_data.shakeDataInited)
		{
			CameraShaker.ShakeEditorCamera(camera, _data.shakeData.intensity, _data.shakeData.duration, _data.shakeData.frequency);
		}

		if (fxPs != null) { DestroyImmediate(fxPs.gameObject); }
		//Generate FX
		if (_data.vfxDataInited && _data.vfxData.vfxPrefab != null)
		{
			GameObject fx = FXManager.InstantiateFX(_data.vfxData.vfxPrefab, _data.vfxData.offset, false, Vector3.up, _data.vfxData.scaleMultiplier);
			instantiatedObjects.Add(fx);
			fxPs = fx.GetComponent<ParticleSystem>();
			fxPs.useAutoRandomSeed = false;
		}
	}
}