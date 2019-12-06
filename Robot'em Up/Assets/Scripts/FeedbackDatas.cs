using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FeedbacksDatas", menuName = "GlobalDatas/FeedbacksDatas", order = 1)]
public class FeedbackDatas : ScriptableObject
{
	public List<FeedbackData> feedbackList = new List<FeedbackData>();
}
