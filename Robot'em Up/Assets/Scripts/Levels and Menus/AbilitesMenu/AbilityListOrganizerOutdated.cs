using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class AbilityListOrganizer : MonoBehaviour
{
    public GameObject[] abilities;
    public float leftMargin = 0;
    public float spaceBetweenBoxes = 14;
    public Color availableColor;
    public Color notAvailableColor;
    private List<RectTransform> availableAbilities = new List<RectTransform>();
    private List<RectTransform> nonAvailableAbilities = new List<RectTransform>();

    public void GetAvailableAbilities()
    {
        availableAbilities.Clear();
        nonAvailableAbilities.Clear();
        foreach (var ability in abilities)
        {
            if (ability.GetComponent<AbilityGroupData>().isBaseUnlocked)
            {
                availableAbilities.Add(ability.GetComponent<RectTransform>());
            }
            else
            {
                nonAvailableAbilities.Add(ability.GetComponent<RectTransform>());
            }
        }
    }

    public void OrganizeAbilities()
    {
        GetAvailableAbilities();

        for (int i = 0; i < availableAbilities.Count; i++)
        {
            float i_YPosition = -(i * spaceBetweenBoxes + i * 22 + 50);
            availableAbilities[i].localPosition = new Vector2(leftMargin, i_YPosition);
            //availableAbilities[i].gameObject.GetComponent<Image>().color = availableColor;
        }

        float i_lastAvailableAbilityY = (availableAbilities[availableAbilities.Count-1].localPosition.y - 11);

        for (int j = 0; j < nonAvailableAbilities.Count; j++)
        {
            float i_YPosition = -(j * spaceBetweenBoxes + j * 22 + spaceBetweenBoxes*2) + i_lastAvailableAbilityY;
            nonAvailableAbilities[j].localPosition = new Vector2(leftMargin, i_YPosition);
            //nonAvailableAbilities[j].gameObject.GetComponent<Image>().color = notAvailableColor;
        }
    }
}
