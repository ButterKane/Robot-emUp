using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class CameraZone : MonoBehaviour
{
	public Vector3 cornerA { get { return m_cornerA; } set { m_cornerA = new Vector3(value.x, transform.position.y, value.z); } }
	[SerializeField]
	private Vector3 m_cornerA = new Vector3(-30, 0f, -30);

	public Vector3 cornerB { get { return m_cornerB; } set { m_cornerB = new Vector3(value.x, transform.position.y, value.z); } }
	[SerializeField]
	private Vector3 m_cornerB = new Vector3(30f, 0f, 30f);


	public int minPlayersRequired = 2;
	public CameraType type;
	private SpriteRenderer visualizer;
	private Transform cameraPivot;
	private Collider collider;
	private bool zoneActivated;
	[HideInInspector] public UnityEvent onZoneActivation;

	private List<PlayerController> playersInside;
#if UNITY_EDITOR
	Tool LastTool = Tool.None;
#endif

	void OnDisable ()
	{
#if UNITY_EDITOR
		Tools.current = LastTool;
#endif
	}
	private void OnEnable ()
	{
#if UNITY_EDITOR
		LastTool = Tools.current;
		Tools.current = Tool.None;
#endif
		if (GetComponent<SpriteRenderer>() == null)
		{
			visualizer = gameObject.AddComponent<SpriteRenderer>();
		} else
		{
			visualizer = GetComponent<SpriteRenderer>();
		}
		visualizer.transform.localRotation = Quaternion.Euler(new Vector3(-90, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z));
		if (cameraPivot == null)
		{
			cameraPivot = transform.parent.Find("Camera pivot");
		}

		playersInside = new List<PlayerController>();
	}

	private void Start ()
	{
#if UNITY_EDITOR
		EditorApplication.playModeStateChanged += RegenerateVisualizer;
#endif
	}
#if UNITY_EDITOR
	private void RegenerateVisualizer ( PlayModeStateChange state )
	{
		if (state == PlayModeStateChange.EnteredPlayMode)
		{
			switch (type)
			{
				case CameraType.Combat:
					visualizer.sprite = null;// Resources.Load<Sprite>("CameraEditor/squareZoneVisualizerIngame");
					break;
				case CameraType.Circle:
					visualizer.sprite = null;// Resources.Load<Sprite>("CameraEditor/circleZoneVisualizerIngame");
					break;
			}
		} else if (state == PlayModeStateChange.ExitingPlayMode)
		{
			switch (type)
			{
				case CameraType.Combat:
					visualizer.sprite = Resources.Load<Sprite>("CameraEditor/squareZoneVisualizer");
					break;
				case CameraType.Circle:
					visualizer.sprite = Resources.Load<Sprite>("CameraEditor/circleZoneVisualizer");
					break;
			}
		}
	}
#endif

	public void GenerateZone(CameraType _type) {
		type = _type;
		switch (type)
		{
			case CameraType.Combat:
				GenerateCombatZone();
				break;
			case CameraType.Circle:
				GenerateCircleZone();
				break;
		}
		collider.isTrigger = true;
	}

	public virtual void Update ()
	{
		if (Application.isPlaying)
		{
			if (IsZoneActivated())
			{
				if (GetPlayersInside().Count * (1 + GameManager.deadPlayers.Count) < minPlayersRequired)
				{
					DesactivateZone();
				}
			}
			else
			{
				if (GetPlayersInside().Count * (1 + GameManager.deadPlayers.Count) >= minPlayersRequired)
				{
					ActivateZone();
				}
			}
		}
		switch (type)
		{
			case CameraType.Combat:
				UpdateCombatZone();
				break;
			case CameraType.Circle:
				UpdateCircleZone();
				break;
		}
	}

	void GenerateCombatZone ()
	{
		visualizer.sprite = Resources.Load<Sprite>("CameraEditor/squareZoneVisualizer");
		visualizer.drawMode = SpriteDrawMode.Tiled;
		visualizer.transform.localScale = new Vector3(30, 30, 30);
		collider = GetComponent<Collider>();
		if (collider == null)
		{
			collider = transform.gameObject.AddComponent<BoxCollider>();
		}
		else
		{
			collider = GetComponent<BoxCollider>();
		}
	}

	void GenerateCircleZone()
	{
		visualizer.sprite = Resources.Load<Sprite>("CameraEditor/circleZoneVisualizer");
		visualizer.drawMode = SpriteDrawMode.Simple;
		visualizer.transform.localScale = new Vector3(30, 30, 30);
		collider = GetComponent<Collider>();
		if (collider == null)
		{
			collider = transform.gameObject.AddComponent<CapsuleCollider>();
		}
		else
		{
			collider = GetComponent<CapsuleCollider>();
		}
	}

	void UpdateCombatZone()
	{
		Vector3 wantedPosition = cornerA + ((cornerB - cornerA) / 2);
		transform.position = new Vector3(wantedPosition.x, transform.position.y, wantedPosition.z);
		if (visualizer != null)
		{
			//Size X: 
			float angleA = Vector3.Angle(transform.TransformDirection(new Vector3(1, 0, 0)), (cornerA - transform.position));
			float sizeX = (cornerA - transform.position).magnitude * Mathf.Cos(angleA * Mathf.Deg2Rad);

			float angleB = Vector3.Angle(transform.TransformDirection(new Vector3(0, 1, 0)), (cornerA - transform.position));
			float sizeY = (cornerA - transform.position).magnitude * Mathf.Cos(angleB * Mathf.Deg2Rad);

			visualizer.size = new Vector3((sizeX * 2) / transform.localScale.x, (sizeY * 2) / transform.localScale.z, 1);
			visualizer.transform.localRotation = Quaternion.Euler(new Vector3(-90, transform.localRotation.eulerAngles.y, 0));
		}
		if (cameraPivot != null && Application.isEditor && !Application.isPlaying)
		{
			cameraPivot.transform.position = wantedPosition;
			cameraPivot.transform.localRotation = Quaternion.Euler(cameraPivot.transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, cameraPivot.transform.localRotation.eulerAngles.z);
		}
		if (collider != null)
		{
			BoxCollider squareCollider = collider as BoxCollider;
			squareCollider.size = new Vector3(Mathf.Abs(visualizer.size.x), Mathf.Abs(visualizer.size.y), 1);
		}
	}

	void UpdateCircleZone ()
	{
		transform.position = visualizer.transform.position;
		if (cameraPivot != null && Application.isEditor && !Application.isPlaying)
		{
			cameraPivot.transform.position = visualizer.transform.position;
		}
	}

	private void OnTriggerEnter ( Collider other )
	{
		PlayerController playerFound = other.GetComponent<PlayerController>();
		if (playerFound != null && !playersInside.Contains(playerFound))
		{
			playersInside.Add(playerFound);
		}
	}

	private void OnTriggerExit ( Collider other )
	{
		PlayerController playerFound = other.GetComponent<PlayerController>();
		if (playerFound != null && playersInside.Contains(playerFound))
		{
			playersInside.Remove(playerFound);
		}
	}

	void ActivateZone()
	{
		zoneActivated = true;
		onZoneActivation.Invoke();
	}

	void DesactivateZone()
	{
		zoneActivated = false;
	}

	public Vector3 GetCenterPosition()
	{
		return transform.position;
	}

	public bool IsZoneActivated()
	{
		return zoneActivated;
	}
	public List<PlayerController> GetPlayersInside()
	{
		if (playersInside.Count > 0)
		{
			if (GameManager.playerOne != null && GameManager.playerOne.moveState == MoveState.Dead && !playersInside.Contains(GameManager.playerOne)) {
				playersInside.Remove(GameManager.playerOne);
			}
			if (GameManager.playerTwo != null && GameManager.playerTwo.moveState == MoveState.Dead && !playersInside.Contains(GameManager.playerTwo))
			{
				playersInside.Remove(GameManager.playerTwo);
			}
		}
		return playersInside;
	}
}