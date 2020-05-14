using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToxicAreaManager : MonoBehaviour
{
    public static ToxicAreaManager i;
    public bool areaActivated = false;
    public bool accelerateDepoisoned = false;
    public float decay_multiplier;
    public float damageWhenPoisened_multiplier;
    public float toxicValue_P1;
    public float toxicValue_P2;
    public bool isPoisened_P1;
    public bool isPoisened_P2;
    public Image poisonedSprite_P1;
    public Image poisonedSprite_P2;
    public Transform playerOneToxicBar;
    public Transform playerTwoToxicBar;
    private Slider playerOneToxicBarSlider;
    private Slider playerTwoToxicBarSlider;

    // Start is called before the first frame update
    void Awake()
    {
        i = this;
        playerOneToxicBar = Instantiate(Resources.Load<GameObject>("PlayerResource/ToxicityIndicator")).transform;
        playerTwoToxicBar = Instantiate(Resources.Load<GameObject>("PlayerResource/ToxicityIndicator")).transform;
    }

        void Start()
        {
            PlayerUI player1UI = GameManager.playerOne.GetComponent<PlayerUI>();
        PlayerUI player2UI = GameManager.playerTwo.GetComponent<PlayerUI>();

        if (player1UI.playerCanvasLateralRectTransform != null)
        {
            playerOneToxicBar.SetParent(player1UI.playerCanvasLateralRectTransform.transform);
        }
        if (player2UI.playerCanvasLateralRectTransform != null)
        {
            playerTwoToxicBar.SetParent(player2UI.playerCanvasLateralRectTransform.transform);
        }

        playerOneToxicBar.transform.localScale = Vector3.one;
        playerTwoToxicBar.transform.localScale = Vector3.one;
        playerOneToxicBar.transform.localPosition = Vector3.zero;
        playerTwoToxicBar.transform.localPosition = Vector3.zero;

        playerOneToxicBarSlider = playerOneToxicBar.GetComponentInChildren<Slider>();
        playerTwoToxicBarSlider = playerTwoToxicBar.GetComponentInChildren<Slider>();

        playerOneToxicBar.gameObject.SetActive(false);
        playerTwoToxicBar.gameObject.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        if (areaActivated)
        {
            //Update slider values
            playerOneToxicBarSlider.value = toxicValue_P1;
            playerTwoToxicBarSlider.value = toxicValue_P2;
            if (toxicValue_P1 >= 1)
            {
                isPoisened_P1 = true;
                toxicValue_P1 = 1;
            }

            if (toxicValue_P1 <= 0)
            {
                toxicValue_P1 = 0;
                isPoisened_P1 = false;
            }


            if (toxicValue_P2 >= 1)
            {
                toxicValue_P2 = 1;
                isPoisened_P2 = true;
            }

            if (toxicValue_P2 <= 0)
            {
                toxicValue_P2 = 0;
                isPoisened_P2 = false;
            }

            toxicValue_P1 -= Time.deltaTime * decay_multiplier;
            toxicValue_P2 -= Time.deltaTime * decay_multiplier;


            if (isPoisened_P1 && GameManager.alivePlayers.Contains(GameManager.playerOne))
            {
                // poisonedSprite_P1.gameObject.SetActive(true);

                GameManager.playerOne.Damage(Time.deltaTime * damageWhenPoisened_multiplier);
                if (accelerateDepoisoned)
                {
                    toxicValue_P1 -= Time.deltaTime * decay_multiplier * 4;
                }
            }
            else
            {
                // poisonedSprite_P1.gameObject.SetActive(false);
            }


            if (isPoisened_P2 && GameManager.alivePlayers.Contains(GameManager.playerTwo))
            {
                //  poisonedSprite_P2.gameObject.SetActive(true);

                GameManager.playerTwo.Damage(Time.deltaTime * damageWhenPoisened_multiplier);
                if (accelerateDepoisoned)
                {
                    toxicValue_P2 -= Time.deltaTime * decay_multiplier * 4;
                }
            }
            else
            {
                //   poisonedSprite_P2.gameObject.SetActive(false);
            }
        }
    }

    public void ToxicAreaEntry()
    {
        Debug.Log("ToxicAreaEntry");
        areaActivated = true;
        playerOneToxicBar.gameObject.SetActive(true);
        playerTwoToxicBar.gameObject.SetActive(true);
    }


    public void ToxicAreaLeaving()
    {
        Debug.Log("ToxicAreaLeaving");
        areaActivated = false;
        playerOneToxicBar.gameObject.SetActive(false);
        playerTwoToxicBar.gameObject.SetActive(false);
    }

}
