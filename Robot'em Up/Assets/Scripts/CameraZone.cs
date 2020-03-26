using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class CameraZone : MonoBehaviour
{
	public Vector3 cornerA_access { get { return m_cornerA; } set { m_cornerA = new Vector3(value.x, transform.position.y, value.z); } }
	[SerializeField]
	private Vector3 m_cornerA = new Vector3(-30, 0f, -30);

	public Vector3 cornerB_access { get { return m_cornerB; } set { m_cornerB = new Vector3(value.x, transform.position.y, value.z); } }
	[SerializeField]
	private Vector3 m_cornerB = new Vector3(30f, 0f, 30f);


	public int minPlayersRequired = 2;
	public bool desactivateCameraOnZoneExit = false;
	public CameraCustomType type;
	private SpriteRenderer visualizer;
	private Transform cameraPivot;
	private Collider genCollider;
	private bool zoneActivated;
	public CameraBehaviour linkedCameraBehaviour;
	[HideInInspector] public UnityEvent onZoneActivation;

	private List<PlayerController> playersInside;
#if UNITY_EDITOR
	Tool lastTool = Tool.None;
#endif

	void OnDisable ()
	{
#if UNITY_EDITOR
		Tools.current = lastTool;
#endif
	}
	private void OnEnable ()
	{
#if UNITY_EDITOR
		lastTool = Tools.current;
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

	[ExecuteAlways]
	private void Awake ()
	{
		playersInside = new List<PlayerController>();
		if (GetComponent<SpriteRenderer>() == null)
		{
			visualizer = gameObject.AddComponent<SpriteRenderer>();
		}
		else
		{
			visualizer = GetComponent<SpriteRenderer>();
		}
		if (Application.isPlaying)
		{
			switch (type)
			{
				case CameraCustomType.Combat:
					visualizer.sprite = null;// Resources.Load<Sprite>("CameraEditor/squareZoneVisualizerIngame");
					break;
				case CameraCustomType.Circle:
					visualizer.sprite = null;// Resources.Load<Sprite>("CameraEditor/circleZoneVisualizerIngame");
					break;
			}
		} else
		{
			switch (type)
			{
				case CameraCustomType.Combat:
					visualizer.sprite = Resources.Load<Sprite>("CameraEditor/squareZoneVisualizer");
					break;
				case CameraCustomType.Circle:
					visualizer.sprite = Resources.Load<Sprite>("CameraEditor/circleZoneVisualizer");
					break;
			}
		}
	}

	private void Start ()
	{
		if (linkedCameraBehaviour == null) { linkedCameraBehaviour = transform.parent.GetComponentInChildren<CameraBehaviour>(); }
	}

	public void GenerateZone( CameraCustomType _type ) {
		type = _type;
		switch (type)
		{
			case CameraCustomType.Combat:
				GenerateCombatZone();
				break;
			case CameraCustomType.Circle:
				GenerateCircleZone();
				break;
		}
		genCollider.isTrigger = true;
	}

	public virtual void Update ()
	{
		if (Application.isPlaying && GameManager.deadPlayers != null)
		{
			if (IsZoneActivated())
			{
				if (GetPlayersInside().Count * (1 + GameManager.deadPlayers.Count) < minPlayersRequired)
				{
					DesactivateZone();
					if (desactivateCameraOnZoneExit) { linkedCameraBehaviour.DesactivateCamera(); }
				}
			}
			else
			{
				if (GetPlayersInside() != null && GetPlayersInside().Count * (1 + GameManager.deadPlayers.Count) >= minPlayersRequired)
				{
					ActivateZone();
					linkedCameraBehaviour.ActivateCamera();
				}
			}
		}
		switch (type)
		{
			case CameraCustomType.Combat:
				UpdateCombatZone();
				break;
			case CameraCustomType.Circle:
				UpdateCircleZone();
				break;
		}
	}

	void GenerateCombatZone ()
	{
		visualizer.sprite = Resources.Load<Sprite>("CameraEditor/squareZoneVisualizer");
		visualizer.drawMode = SpriteDrawMode.Tiled;
		visualizer.transform.localScale = new Vector3(30, 30, 30);
		genCollider = GetComponent<Collider>();
		if (genCollider == null)
		{
			genCollider = transform.gameObject.AddComponent<BoxCollider>();
		}
		else
		{
			genCollider = GetComponent<BoxCollider>();
		}
	}

	void GenerateCircleZone()
	{
		visualizer.sprite = Resources.Load<Sprite>("CameraEditor/circleZoneVisualizer");
		visualizer.drawMode = SpriteDrawMode.Simple;
		visualizer.transform.localScale = new Vector3(30, 30, 30);
		genCollider = GetComponent<Collider>();
		if (genCollider == null)
		{
			genCollider = transform.gameObject.AddComponent<CapsuleCollider>();
		}
		else
		{
			genCollider = GetComponent<CapsuleCollider>();
		}
	}

	void UpdateCombatZone()
	{
		Vector3 i_wantedPosition = cornerA_access + ((cornerB_access - cornerA_access) / 2);
		transform.position = new Vector3(i_wantedPosition.x, transform.position.y, i_wantedPosition.z);
		if (visualizer != null)
		{
			//Size X: 
			float angleA = Vector3.Angle(transform.TransformDirection(new Vector3(1, 0, 0)), (cornerA_access - transform.position));
			float sizeX = (cornerA_access - transform.position).magnitude * Mathf.Cos(angleA * Mathf.Deg2Rad);

			float angleB = Vector3.Angle(transform.TransformDirection(new Vector3(0, 1, 0)), (cornerA_access - transform.position));
			float sizeY = (cornerA_access - transform.position).magnitude * Mathf.Cos(angleB * Mathf.Deg2Rad);

			visualizer.size = new Vector3((sizeX * 2) / transform.localScale.x, (sizeY * 2) / transform.localScale.z, 1);
			visualizer.transform.localRotation = Quaternion.Euler(new Vector3(-90, transform.localRotation.eulerAngles.y, 0));
		}
		if (cameraPivot != null && Application.isEditor && !Application.isPlaying)
		{
			cameraPivot.transform.position = i_wantedPosition;
			cameraPivot.transform.localRotation = Quaternion.Euler(cameraPivot.transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, cameraPivot.transform.localRotation.eulerAngles.z);
		}
		if (genCollider != null)
		{
			BoxCollider squareCollider = genCollider as BoxCollider;
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

	private void OnTriggerEnter ( Collider _other )
	{
		PlayerController i_playerFound = _other.GetComponent<PlayerController>();
		if (i_playerFound != null && !playersInside.Contains(i_playerFound))
		{
			playersInside.Add(i_playerFound);
		}
	}

	private void OnTriggerExit ( Collider _other )
	{
		PlayerController i_playerFound = _other.GetComponent<PlayerController>();
		if (i_playerFound != null && playersInside.Contains(i_playerFound))
		{
			playersInside.Remove(i_playerFound);
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