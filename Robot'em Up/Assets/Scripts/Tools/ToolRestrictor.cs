using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class ToolRestrictor : MonoBehaviour
{
	public List<Tool> restrictedTools;
	Tool LastTool = Tool.None;
	void OnDisable ()
	{
		Tools.current = LastTool;
	}
	private void OnEnable ()
	{
		LastTool = Tools.current;
		Tools.current = Tool.None;
	}
}

[CustomEditor(typeof(ToolRestrictor)), CanEditMultipleObjects]
public class ToolRestrictorEditor : Editor
{
	protected virtual void OnSceneGUI ()
	{
		ToolRestrictor toolRestrictor = (ToolRestrictor)target;
		if (toolRestrictor.restrictedTools.Contains(Tools.current))
		{
			Tools.current = Tool.None;
		}
	}
}
#endif