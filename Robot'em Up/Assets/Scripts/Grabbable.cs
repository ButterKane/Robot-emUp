using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
	public List<GrabbableInformation> grabbableInformation = new List<GrabbableInformation>();

	public void UpdateLine()
	{
		foreach (GrabbableInformation gi in grabbableInformation)
		{
			if (gi.trigger != null && gi.previewLine != null)
			{
				gi.previewLine.positionCount = 2;
				gi.previewLine.SetPosition(0, gi.trigger.center + gi.trigger.transform.position);
				gi.previewLine.SetPosition(1, gi.targetedPosition.position);
			}
		}
	}
}

public class PlayerGrabPreview : MonoBehaviour
{
	public PlayerController player;
	public LineRenderer lr;
}
public class GrabbableInformation : MonoBehaviour
{
	public int id;
	public SphereCollider trigger;
	public Transform targetedPosition;
	public LineRenderer previewLine;
	private Transform previewUI;
	private bool previewEnabled;
	private List<PlayerGrabPreview> playerGrabPreview = new List<PlayerGrabPreview>();
	private GrabbableDatas datas;

	private void Awake ()
	{
		datas = GrabbableDatas.GetDatas();
		previewLine.material = datas.ingameLineRendererMaterial;
		previewLine.startWidth = datas.ingameLineRendererWidth;
		previewLine.endWidth = datas.ingameLineRendererWidth;
		previewUI = Instantiate(datas.grabAvailableUIPreview).transform;
		previewUI.transform.SetParent(this.transform);
		previewEnabled = false;
		if (previewUI != null)
		{
			previewLine.enabled = false;
			previewUI.gameObject.SetActive(false);
		}
	}
	private void Update ()
	{
		if (previewUI != null && previewEnabled)
		{
			previewUI.transform.position = GameManager.mainCamera.WorldToScreenPoint(targetedPosition.position);
		}
		foreach (PlayerGrabPreview p in playerGrabPreview)
		{
			if (!p.player.extendingArmsController.previewShown)
			{
				p.lr.positionCount = 2;
				p.lr.SetPosition(0, p.player.GetCenterPosition());
				p.lr.SetPosition(1, targetedPosition.position);
			} else
			{
				p.lr.positionCount = 0;
			}
		}
	}

	public void EnablePreviewForPlayer(PlayerController player)
	{
		previewEnabled = true;
		if (previewUI != null)
		{
			previewUI.transform.SetParent(GameManager.mainCanvas.transform);
			//previewLine.enabled = true;
			previewUI.gameObject.SetActive(true);
		}
		PlayerGrabPreview pgp = new GameObject().AddComponent<PlayerGrabPreview>();
		pgp.gameObject.name = "PlayerGrabPreview";
		pgp.transform.SetParent(transform);
		pgp.transform.localPosition = Vector3.zero;
		pgp.player = player;
		pgp.lr = pgp.gameObject.AddComponent<LineRenderer>();
		pgp.lr.material = datas.ingameLineRendererMaterial;
		pgp.lr.startWidth = datas.ingameLineRendererWidth;
		pgp.lr.endWidth = datas.ingameLineRendererWidth;
		playerGrabPreview.Add(pgp);
	}

	public void DisablePreviewForPlayer(PlayerController player)
	{
		previewEnabled = false;
		if (previewUI != null)
		{
			previewUI.transform.SetParent(transform);
			//previewLine.enabled = false;
			previewUI.gameObject.SetActive(false);
			player.extendingArmsController.TogglePreview(false);
		}
		PlayerGrabPreview foundPreview = null;
		foreach (PlayerGrabPreview p in playerGrabPreview)
		{
			if (p.player == player)
			{
				foundPreview = p;
			}
		}
		if (foundPreview != null)
		{
			playerGrabPreview.Remove(foundPreview);
			Destroy(foundPreview.gameObject);
		}
	}
}
