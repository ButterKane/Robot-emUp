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
    public float toxicity_multiplier;
    public float damageWhenPoisened_multiplier;
    public float toxicValue_P1;
    public float toxicValue_P2;
    public bool isPoisened_P1;
    public bool isPoisened_P2;
    public int isInToxicArea_P1;
    public int isInToxicArea_P2;
    public Image poisonedSprite_P1;
    public Image poisonedSprite_P2;
    public Transform playerOneToxicBar;
    public Transform playerTwoToxicBar;
    private Slider playerOneToxicBarSlider;
    private Slider playerTwoToxicBarSlider;
    private float inflictDamage_P1;
    private float inflictDamage_P2;

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

        playerOneToxicBar.transform.localRotation = Quaternion.identity;
        playerTwoToxicBar.transform.localRotation = Quaternion.identity;
        playerOneToxicBar.transform.localScale = Vector3.one;
        playerTwoToxicBar.transform.localScale = Vector3.one;
        playerOneToxicBar.transform.localPosition = Vector3.zero;
        playerTwoToxicBar.transform.localPosition = Vector3.zero;

        playerOneToxicBarSlider = playerOneToxicBar.GetComponentInChildren<Slider>();
        playerTwoToxicBarSlider = playerTwoToxicBar.GetComponentInChildren<Slider>();

        playerOneToxicBar.gameObject.SetActive(false);
        playerTwoToxicBar.gameObject.SetActive(false);

        decay_multiplier = 0.6f;
        damageWhenPoisened_multiplier = 5;
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
            print(decay_multiplier);

            //UP AND DOWN TOXICITY VALUE
            if (isInToxicArea_P1>0)
            {
                toxicValue_P1 += Time.deltaTime * toxicity_multiplier;
            }
            else
            {
                toxicValue_P1 -= Time.deltaTime * decay_multiplier;
            }
            if (isInToxicArea_P2>0)
            {
                toxicValue_P2 += Time.deltaTime * toxicity_multiplier;
            }
            else
            {
                toxicValue_P2 -= Time.deltaTime * decay_multiplier;
            }

            inflictDamage_P1 -= Time.deltaTime;
            inflictDamage_P2 -= Time.deltaTime;

            if (isPoisened_P1 && GameManager.alivePlayers.Contains(GameManager.playerOne) && inflictDamage_P1<0)
            {
                // poisonedSprite_P1.gameObject.SetActive(true);
                inflictDamage_P1 = 0.5f;
                GameManager.playerOne.Damage(damageWhenPoisened_multiplier);
            }
            else
            {
                // poisonedSprite_P1.gameObject.SetActive(false);
            }


            if (isPoisened_P2 && GameManager.alivePlayers.Contains(GameManager.playerTwo) && inflictDamage_P2 < 0)
            {
                //  poisonedSprite_P2.gameObject.SetActive(true);
                inflictDamage_P2 = 0.5f;

                GameManager.playerTwo.Damage(damageWhenPoisened_multiplier);
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
