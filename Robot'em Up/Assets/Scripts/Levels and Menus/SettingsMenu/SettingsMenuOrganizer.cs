using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SettingsMenuOrganizer : MonoBehaviour
{
    public float leftMargin = 10;
    public float spaceBetweenBoxes = 20;
    public float boxesHeight = 40;
    public float boxesWidth = 470;
    [ReadOnly] public RectTransform[] settingsToOrganize;
    [ReadOnly] public GameObject[] childrenObjects;
    [ReadOnly] public UIBehaviour selectedSettingInChildren;
    public Image quarantineRibbon;
    public bool[] settingsToPutInQuarantine;
    [ReadOnly] public List<RectTransform> availableSettingsToOrganize = new List<RectTransform>();
    private List<RectTransform> quarantinedSettingsToOrganize = new List<RectTransform>();
    private GameObject[] selectionableChildrenObjects;

    private void Awake()
    {
        gameObject.SetActive(true);
        GetChildren();
        OrganizeSettingsDisplay();
    }

    private void Update()
    {
        
    }

    public void OrganizeSettingsDisplay()
    {
        int nbOfSettingsInOfficial = 0;
        int nbOfSettingsInQuarantine = 0;
        quarantinedSettingsToOrganize.Clear();
        availableSettingsToOrganize.Clear();
        for (int i = 0; i < settingsToOrganize.Length; i++)
        {
            if (i < settingsToPutInQuarantine.Length && settingsToPutInQuarantine[i] == true)
            {
                quarantinedSettingsToOrganize.Add(settingsToOrganize[i]);
                nbOfSettingsInQuarantine++;
            }
            else
            {
                availableSettingsToOrganize.Add(settingsToOrganize[i]);
                nbOfSettingsInOfficial++;
            }
        }


        selectionableChildrenObjects = new GameObject[availableSettingsToOrganize.Count];
        for (int j = 0; j < availableSettingsToOrganize.Count; j++)
        {
            float i_YPosition = -(j * boxesHeight + j * spaceBetweenBoxes);
            availableSettingsToOrganize[j].localPosition = new Vector2(leftMargin, i_YPosition);
            availableSettingsToOrganize[j].sizeDelta = new Vector2(boxesWidth, boxesHeight);

            selectionableChildrenObjects[j] = availableSettingsToOrganize[j].gameObject;
        }

        if (nbOfSettingsInQuarantine == 0) { quarantineRibbon.enabled = false; return; }
        else
        {
            quarantineRibbon.enabled = true;
            if (availableSettingsToOrganize.Count > 0) { quarantineRibbon.rectTransform.localPosition = new Vector2(leftMargin, availableSettingsToOrganize[availableSettingsToOrganize.Count - 1].localPosition.y - 20); }
            else { quarantineRibbon.rectTransform.localPosition = new Vector2(leftMargin, 0); }
            
            quarantineRibbon.rectTransform.sizeDelta = new Vector2(boxesWidth, boxesHeight*2);

            float i_NewYRef = quarantineRibbon.rectTransform.localPosition.y - 40;

            for (int z = 0; z < quarantinedSettingsToOrganize.Count; z++)
            {
                float i_YPosition = i_NewYRef - (z * boxesHeight + z * spaceBetweenBoxes);
                quarantinedSettingsToOrganize[z].localPosition = new Vector2(leftMargin, i_YPosition);

                quarantinedSettingsToOrganize[z].sizeDelta = new Vector2(boxesWidth, boxesHeight);
            }
        }
    }

    public void GetChildren()
    {
        settingsToOrganize = new RectTransform[transform.childCount-1]; // -1 because of the ribbon
        childrenObjects = new GameObject[transform.childCount-1];
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<UIBehaviour>())
            {
                childrenObjects[i] = transform.GetChild(i).gameObject;
                settingsToOrganize[i] = transform.GetChild(i).GetComponent<RectTransform>();
            }
        }
    }

    public UIBehaviour SelectSetting(int _index)
    {
        UIBehaviour settingScriptToReturn = null;

        for (int i = 0; i < selectionableChildrenObjects.Length; i++)
        {
            if (i == _index)
            {
                settingScriptToReturn = selectionableChildrenObjects[i].GetComponent<UIBehaviour>().SelectThisSetting();
                selectedSettingInChildren = settingScriptToReturn;
            }
            else
            {
                UIBehaviour foundBehaviour = selectionableChildrenObjects[i].GetComponent<UIBehaviour>();
                if (foundBehaviour != null)
                {
                    selectionableChildrenObjects[i].GetComponent<UIBehaviour>().UnselectThisSetting();
                }
            }
        }
        return settingScriptToReturn;
    }

    public void GetQuarantinableSettings()
    {
        GetChildren();
        settingsToPutInQuarantine = new bool[settingsToOrganize.Length];
    }

    public void OrganizeEverything()
    {
        OrganizeSettingsDisplay();
    }
}
