using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityGroupData : MonoBehaviour
{
    [Separator("References")]
    public Image backgroundImage;
    public TextMeshProUGUI text;

    [Separator("Ability Datas")]
    public ConcernedAbility ability;
    public string abilityName;
    [TextArea(5, 10)]
    public string abilityDescription;
    [TextArea(2, 10)]
    public string upgrade1Description;
    [TextArea(2, 10)]
    public string upgrade2Description;

    public Sprite[] gifImages;
    public bool isBaseUnlocked = true;
    public bool isUpgrade1Unlocked = false;
    public bool isUpgrade2Unlocked = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
