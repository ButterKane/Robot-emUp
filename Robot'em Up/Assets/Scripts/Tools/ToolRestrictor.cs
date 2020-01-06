using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class ToolRestrictor : MonoBehaviour
{
	public List<Tool> restrictedTools;
	Tool lastTool = Tool.None;
	void OnDisable ()
	{
		Tools.current = lastTool;
	}
	private void OnEnable ()
	{
		lastTool = Tools.current;
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