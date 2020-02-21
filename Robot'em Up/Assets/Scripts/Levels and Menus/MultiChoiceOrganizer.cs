using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MultiChoiceOrganizer : MonoBehaviour
{
    public Vector2 minChoicesSize = new Vector2(7, 10);
    public Vector2 maxChoicesSize = new Vector2(50, 10);
    private RectTransform thisRect;

    [ReadOnly] public List<RectTransform> choicesToOrganize;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(thisRect == null)
        {
            thisRect = GetComponent<RectTransform>();
        }

        GetChildren();
        OrganizeSettingsDisplay();
    }

    public void OrganizeSettingsDisplay()
    {
        int i_howManyBoxes = choicesToOrganize.Count;
        float i_sectionSize = thisRect.rect.width / i_howManyBoxes;

        for (int i = 0; i < choicesToOrganize.Count; i++)
        {
            float i_XPosition = i * i_sectionSize + i_sectionSize/2;

            choicesToOrganize[i].anchoredPosition = new Vector2(i_XPosition, choicesToOrganize[i].anchoredPosition.y);
            Debug.Log("supposed positionX of object is " + i_XPosition + " and actual position is " + choicesToOrganize[i].anchoredPosition);

            choicesToOrganize[i].sizeDelta = new Vector2(Mathf.Lerp(minChoicesSize.x, maxChoicesSize.x, i_sectionSize/ thisRect.rect.width), Mathf.Lerp(minChoicesSize.y, maxChoicesSize.y, i_sectionSize / thisRect.rect.width));
        }
    }

    public void GetChildren()
    {
        choicesToOrganize = new List<RectTransform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Image i_checkingIfIsChoiceObject = transform.GetChild(i).GetComponent<Image>();
            if (i_checkingIfIsChoiceObject != null)
            {
                choicesToOrganize.Add(i_checkingIfIsChoiceObject.rectTransform);
            }
        }
    }
}
