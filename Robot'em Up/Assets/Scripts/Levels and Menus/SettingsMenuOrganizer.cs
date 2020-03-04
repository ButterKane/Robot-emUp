using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SettingsMenuOrganizer : MonoBehaviour
{
    public float leftMargin = 10;
    public float spaceBetweenBoxes = 20;
    public float boxesHeight = 40;
    public float boxesWidth = 470;
    [ReadOnly] public RectTransform[] settingsToOrganize;
    [ReadOnly] public GameObject[] childrenObjects;

    private void Awake()
    {
        gameObject.SetActive(true);
        GetChildren();
        OrganizeSettingsDisplay();

    }

    private void Update()
    {
        GetChildren();
        OrganizeSettingsDisplay();
    }

    public void OrganizeSettingsDisplay()
    {
        for (int i = 0; i < settingsToOrganize.Length; i++)
        {
            float i_YPosition = -(i * boxesHeight + i * spaceBetweenBoxes);
            settingsToOrganize[i].localPosition = new Vector2(leftMargin, i_YPosition);

            settingsToOrganize[i].sizeDelta = new Vector2(boxesWidth, boxesHeight);
        }
    }

    public void GetChildren()
    {
        settingsToOrganize = new RectTransform[transform.childCount];
        childrenObjects = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            childrenObjects[i] = transform.GetChild(i).gameObject;
            settingsToOrganize[i] = transform.GetChild(i).GetComponent<RectTransform>();
        }
    }

    public UIBehaviour SelectSetting(int _index)
    {
        UIBehaviour settingScriptToReturn = null;

        for (int i = 0; i < childrenObjects.Length; i++)
        {
            if (i == _index)
            {
                settingScriptToReturn = childrenObjects[i].GetComponent<UIBehaviour>().SelectThisSetting();
            }
            else
            {
                childrenObjects[i].GetComponent<UIBehaviour>().UnselectThisSetting();
            }
        }
        return settingScriptToReturn;
    }
}
