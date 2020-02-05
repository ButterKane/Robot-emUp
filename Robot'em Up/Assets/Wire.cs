using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Wire : MonoBehaviour
{
	private LineRenderer lr;
	public Color defaultColor = Color.white;
	public Color activatedColor = Color.cyan;
	public float width = 0.1f;
	public float activationSpeed = 3;
	public float desactivationSpeed = 50;
	private float activationPercent;

	private void Awake ()
	{
		lr = GetComponent<LineRenderer>();
	}
	public void Init ()
	{
		lr = gameObject.AddComponent<LineRenderer>();
		lr.sharedMaterial = Resources.Load<Material>("PuzzleResource/M_WireMaterial");
		lr.useWorldSpace = false;
		//lr.alignment = LineAlignment.TransformZ;
		lr.SetPosition(1, transform.position + Vector3.forward * 15 + Vector3.up * 0.01f);
	}

	public void ApplySettings()
	{
		if (lr == null) { lr = GetComponent<LineRenderer>(); }
		lr.sharedMaterial = Resources.Load<Material>("PuzzleResource/M_WireMaterial");
		lr.startWidth = width;
		lr.endWidth = width;
		lr.sharedMaterial.SetColor("_LinkChargedColor", activatedColor);
		lr.sharedMaterial.SetColor("_LinkUnchargedColor", defaultColor);
		lr.sharedMaterial.SetFloat("_Progression", 0);
	}

	public void ActivateWire(UnityAction _callBack)
	{
		StopAllCoroutines();
		StartCoroutine(ActivateWire_C(_callBack));

	}

	public void DesactivateWire ( UnityAction _callBack )
	{
		StopAllCoroutines();
		StartCoroutine(DesactivateWire_C(_callBack));
	}

	IEnumerator ActivateWire_C ( UnityAction _callBack )
	{
		for (float i = 0; i < 1; i+= Time.deltaTime * activationSpeed)
		{
			activationPercent = i;
			lr.sharedMaterial.SetFloat("_Progression", i / 1f);
			yield return null;
		}
		lr.sharedMaterial.SetFloat("_Progression", 1f);
		_callBack.Invoke();
	}

	IEnumerator DesactivateWire_C ( UnityAction _callBack )
	{
		for (float i = activationPercent; i > 0; i -= Time.deltaTime * desactivationSpeed)
		{
			activationPercent = i;
			lr.sharedMaterial.SetFloat("_Progression", i / 1f);
			yield return null;
		}
		lr.sharedMaterial.SetFloat("_Progression", 0f);
		_callBack.Invoke();
	}
}
