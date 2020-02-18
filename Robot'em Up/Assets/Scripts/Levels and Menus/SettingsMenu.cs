using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    //private List<GameObject> categories = new List<GameObject>();

    public List<GameObject> menuCategories = new List<GameObject>();
    private GameObject selectedCategory;
    private int selectedCategoryIndex;
    private bool waitForJoystickResetOne;
    private bool waitForJoystickResetTwo;

    private bool waitForAResetOne;
    private bool waitForAResetTwo;

    private int categoryNumber;


    // Start is called before the first frame update
    void Start()
    {
        selectedCategoryIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ChangeCategory(int _plusOrMinus)
    {
        int i_addition = (int)Mathf.Sign(_plusOrMinus);
        if (selectedCategoryIndex + i_addition < menuCategories.Count && selectedCategoryIndex + i_addition >= 0)
        {
            selectedCategoryIndex += i_addition;
        }
        else
        {
            // Play "error" sound
        }
    }

    void DisplayCategorySettings()
    {
        for (int i = 0; i < menuCategories.Count-1; i++)
        {
            if (i == selectedCategoryIndex)
            {
                menuCategories[i].SetActive(true);
                selectedCategory = menuCategories[i];
            }
            else
            {
                menuCategories[i].SetActive(false);
            }
        }


    }

    // TODO: make the navigation available.
    // Joystick = settings navigation an modification
    // LB & RB = Category navigation
    // Each Category changing makes the available settings change, but the easy way to do it is to get the UIBehaviour Script of each category main object, and store the settings in it.

}
