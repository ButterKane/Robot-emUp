using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
	public Color activatedColor = Color.green;
	public Color waitingColor = new Color(1f, 0.46f, 0.2f, 1f);
	public Color desactivatedColor = Color.red;
	public float spaceBetweenCounters = 0.5f;
	public float secondaryCounterSizeModifier = 0.6f;
	public float activatedColorIntensity = 10f;
	public float activatedColorMaxIntensity = 15f;
	public AnimationCurve activatedColorAnimationCurve;
	public float activatedColorAnimationDuration = 0.5f;
	public float desactivatedColorIntensity = 5f;
	public float waitingColorIntensity = 7f;

	public bool isWaveCounter = false;
	public List<GameObject> counterList;
	private int currentActivatedIndex = 0;
	private bool counterEnd = false;
	private void Awake ()
	{
		if (counterList == null) { return; }
		foreach (GameObject counter in counterList)
		{
			counter.GetComponent<MeshRenderer>().sharedMaterial = new Material(counter.GetComponent<MeshRenderer>().sharedMaterial);
			Material matInstance = counter.GetComponent<MeshRenderer>().material;
			matInstance.SetColor("_EmissionColor", desactivatedColor * desactivatedColorIntensity);
			matInstance.EnableKeyword("_EMISSION");
		}
	}

	[ExecuteAlways]
	private void OnDestroy ()
	{
		if (counterList != null) { foreach (GameObject obj in counterList) { DestroyImmediate(obj); } }
	}

	public void SetCounterToWaiting()
	{
		if (counterEnd) { return; }
		int index = currentActivatedIndex+1;
		if (index > counterList.Count) { return; }
		Material matInstance = counterList[index].GetComponent<MeshRenderer>().material;
		matInstance.SetColor("_EmissionColor", waitingColor * waitingColorIntensity);
	}

	void SetCounterToGreen(GameObject _counter )
	{
		StartCoroutine(SetCounterToGreen_C(_counter));
	}

	IEnumerator SetCounterToGreen_C( GameObject _counter )
	{
		Material matInstance = _counter.GetComponent<MeshRenderer>().material;
		matInstance.SetColor("_EmissionColor", activatedColor * activatedColorIntensity);
		for (float i = 0; i < activatedColorAnimationDuration; i+= Time.deltaTime)
		{
			float colorIntensity = Mathf.Lerp(activatedColorIntensity, activatedColorMaxIntensity, activatedColorAnimationCurve.Evaluate(i / activatedColorAnimationDuration));
			matInstance.SetColor("_EmissionColor", activatedColor * colorIntensity);
			yield return null;
		}
	}

	public void IncreaseCounter(int _amount)
	{
		if (counterEnd) { return; }
		for (int i = 0; i < _amount; i++)
		{
			currentActivatedIndex += 1;
			if (currentActivatedIndex >= counterList.Count) { return; }
			SetCounterToGreen(counterList[currentActivatedIndex]);
			if (currentActivatedIndex >= counterList.Count - 1 ) { currentActivatedIndex = 0; SetCounterToGreen(counterList[currentActivatedIndex]); }
			if (isWaveCounter)
			{
				FeedbackManager.SendFeedback("event.WaveCounterIncreasing", this); 
			}
			if (currentActivatedIndex == 0)
			{
				FeedbackManager.SendFeedback("event.WaveCounterFinished", this);
				SetCounterToGreen(counterList[0]);
				counterEnd = true;
			}
		}
	}

	[ExecuteAlways]
	public void Generate(int _maxCount)
	{
		if (counterList != null) { foreach (GameObject obj in counterList) { DestroyImmediate(obj); } }
		counterList = new List<GameObject>();
		GameObject mainCounter = Instantiate(Resources.Load<GameObject>("ArenaResource/Counter"), this.transform);
		mainCounter.name = "Counter_Main";
		mainCounter.transform.localPosition = new Vector3(0, 0.7f,-0.2f);
		counterList.Add(mainCounter);
		for (int i = 0; i < _maxCount; i++)
		{
			GameObject secondaryCounter = Instantiate(Resources.Load<GameObject>("ArenaResource/Counter"), this.transform);
			secondaryCounter.name = "Counter_" + i;
			secondaryCounter.transform.localScale = secondaryCounter.transform.localScale * secondaryCounterSizeModifier;
			float xWidth = _maxCount * spaceBetweenCounters;
			float xPos = ((_maxCount - ((_maxCount)-i)) * spaceBetweenCounters) - (xWidth / 2f) + (spaceBetweenCounters/2f);
			secondaryCounter.transform.localPosition = new Vector3(xPos, -0.5f, -0.2f);
			counterList.Add(secondaryCounter);
		}
		currentActivatedIndex = 0;
	}
}
