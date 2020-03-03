using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToxicAreaManager : MonoBehaviour
{
    public bool areaActivated = false;
    public bool accelerateDepoisened = false;
    public float decay_multiplier;
    public float damageWhenPoisened_multiplier;
    public float ToxicValue_P1;
    public float ToxicValue_P2;
    public bool isPoisened_P1;
    public bool isPoisened_P2;
    public Image PoisonedSprite_P1;
    public Image PoisonedSprite_P2;
    public Slider PlayerOneToxicBar;
    public Slider PlayerTwoToxicBar;

    // Start is called before the first frame update
    void Start()
    {
        PlayerOneToxicBar.gameObject.SetActive(false);
        PlayerTwoToxicBar.gameObject.SetActive(false);
        PoisonedSprite_P1.gameObject.SetActive(false);
        PoisonedSprite_P2.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerOneToxicBar.value = ToxicValue_P1;
        PlayerTwoToxicBar.value = ToxicValue_P2;
        if (ToxicValue_P1 >= 1)
        {
            isPoisened_P1 = true;
            ToxicValue_P1 = 1;
        }

        if (ToxicValue_P1 <= 0)
        {
            ToxicValue_P1 = 0;
            isPoisened_P1 = false;
        }


        if (ToxicValue_P2 >= 1)
        {
            ToxicValue_P2 = 1;
            isPoisened_P2 = true;
        }

        if (ToxicValue_P2 <= 0)
        {
            ToxicValue_P2 = 0;
            isPoisened_P2 = false;
        }

        ToxicValue_P1 -= Time.deltaTime * decay_multiplier;
        ToxicValue_P2 -= Time.deltaTime * decay_multiplier;


        if (isPoisened_P1)
        {
            PoisonedSprite_P1.gameObject.SetActive(true);

            GameManager.playerOne.Damage(Time.deltaTime * damageWhenPoisened_multiplier);
            if (accelerateDepoisened)
            {
                ToxicValue_P1 -= Time.deltaTime * decay_multiplier * 3;
            }
        }
        else
        {
            PoisonedSprite_P1.gameObject.SetActive(false);
        }


        if (isPoisened_P2)
        {
            PoisonedSprite_P2.gameObject.SetActive(true);

            GameManager.playerTwo.Damage(Time.deltaTime * damageWhenPoisened_multiplier);
            if (accelerateDepoisened)
            {
                ToxicValue_P2 -= Time.deltaTime * decay_multiplier * 3;
            }
        }
        else
        {
            PoisonedSprite_P2.gameObject.SetActive(false);
        }
    }

    public void ToxicAreaEntry()
    {
        Debug.Log("ToxicAreaEntry");
        areaActivated = true;
        PlayerOneToxicBar.gameObject.SetActive(true);
        PlayerTwoToxicBar.gameObject.SetActive(true);
    }


    public void ToxicAreaLeaving()
    {
        Debug.Log("ToxicAreaLeaving");
        areaActivated = false;
        PlayerOneToxicBar.gameObject.SetActive(false);
        PlayerTwoToxicBar.gameObject.SetActive(false);
    }

}
