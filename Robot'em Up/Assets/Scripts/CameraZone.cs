using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CameraZone : MonoBehaviour
{
	public Vector3 cornerA { get { return m_cornerA; } set { m_cornerA = new Vector3(value.x, transform.position.y, value.z); } }
	[SerializeField]
	private Vector3 m_cornerA = new Vector3(-30, 0f, -30);

	public Vector3 cornerB { get { return m_cornerB; } set { m_cornerB = new Vector3(value.x, transform.position.y, value.z); } }
	[SerializeField]
	private Vector3 m_cornerB = new Vector3(30f, 0f, 30f);


	private SpriteRenderer visualizer;
	private Transform cameraPivot;
	private BoxCollider boxCollider;

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
		visualizer.transform.localScale = new Vector3(30, 30, 30);
		visualizer.sprite = Resources.Load<Sprite>("CameraEditor/zoneVisualizer");
		visualizer.drawMode = SpriteDrawMode.Tiled;
		if (GetComponent<BoxCollider>() == null)
		{
			boxCollider = transform.gameObject.AddComponent<BoxCollider>();
		} else
		{
			boxCollider = GetComponent<BoxCollider>();
		}
		boxCollider.isTrigger = true;
		playersInside = new List<PlayerController>();
	}

	public virtual void Update ()
	{
		Vector3 wantedPosition = cornerA + ((cornerB- cornerA) / 2);
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
		if (cameraPivot == null)
		{
			cameraPivot = transform.parent.Find("Camera pivot");
		}
		if (cameraPivot != null && Application.isEditor && !Application.isPlaying)
		{
			cameraPivot.transform.position = wantedPosition;
			cameraPivot.transform.localRotation = Quaternion.Euler(cameraPivot.transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, cameraPivot.transform.localRotation.eulerAngles.z);
		}
		if (boxCollider != null)
		{
			boxCollider.size = new Vector3(Mathf.Abs(visualizer.size.x), Mathf.Abs(visualizer.size.y), 1); 
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