using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenuTab : MonoBehaviour
{
	public UIBehaviour[] childrens;

	private void Start ()
	{
		childrens = gameObject.GetComponentsInChildren<UIBehaviour>();
		if (childrens != null && childrens.Length > 0)
			SettingsMenuOld.SetSelectedSetting(childrens[0].SelectThisSetting());
	}
}
