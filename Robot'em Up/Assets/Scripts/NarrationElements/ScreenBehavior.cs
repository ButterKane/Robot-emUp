using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenBehavior : MonoBehaviour, IHitable
{

    Transform player1Transform;
    Transform player2Transform;
    public Transform leftEyeTransform;
    public Transform rightEyeTransform;
    public Transform topEyeTransform;
    public float maxEyeOffset;
    Vector3 eyesWantedPosition;
    public enum WhereToLook { Left, Middle, Right };
    WhereToLook whereToLook = WhereToLook.Middle;
    public bool bugging;
    bool broken;
    public bool possessed;
    public Renderer myRend;
    public Material possessedMat;
    public Material brokenScreenMat;
    public Material notPossessedMat;
    public float eyeLerpIntensity;

    //Possession variables
    public float possessionAnimationMaxTime;
    public AnimationCurve possessionAnimationCurve;
    float possessionAnimationTimer;
    public Light possessionLight;
    public float maxLightIntensity;

    [SerializeField] protected bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
    [SerializeField] protected float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

    // Start is called before the first frame update
    void Start()
    {
        player1Transform = GameManager.playerOne.transform;
        player2Transform = GameManager.playerTwo.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!broken)
        {
            UpdateEyeLocation();

            if (bugging)
            {
                BuggingMatModification();
            }
        }
    }

    public void SetAIPossession(bool _isPossessed)
    {
        if (_isPossessed && !broken)
        {
            possessed = true;
            possessionAnimationTimer = 0;
            StartCoroutine(SetPossession( true));
        }
        else if(!broken)
        {
            possessed = false;
            possessionAnimationTimer = 0;
            StartCoroutine(SetPossession(false));
        }
    }

    IEnumerator SetPossession(bool _isPossessed)
    {
        if (_isPossessed) //IS POSSESSED
        {
            possessionAnimationTimer += Time.deltaTime;
            possessionLight.intensity = Mathf.Lerp(0, maxLightIntensity, possessionAnimationCurve.Evaluate(possessionAnimationTimer / possessionAnimationMaxTime));
            yield return new WaitForEndOfFrame();
            if (possessionAnimationTimer < possessionAnimationMaxTime)
            {
                StartCoroutine(SetPossession(true));
            }
            else
            {
                myRend.material = possessedMat;
                topEyeTransform.gameObject.SetActive(true);
                leftEyeTransform.gameObject.SetActive(true);
                rightEyeTransform.gameObject.SetActive(true);
            }
        }
        else // NOT POSSESSED
        {
            possessionAnimationTimer += Time.deltaTime;
            possessionLight.intensity = Mathf.Lerp(0, maxLightIntensity, possessionAnimationCurve.Evaluate(possessionAnimationTimer / possessionAnimationMaxTime));
            yield return new WaitForEndOfFrame();
            if (possessionAnimationTimer < possessionAnimationMaxTime)
            {
                StartCoroutine(SetPossession(false));
            }
            else
            {
                myRend.material = notPossessedMat;
                topEyeTransform.gameObject.SetActive(false);
                leftEyeTransform.gameObject.SetActive(false);
                rightEyeTransform.gameObject.SetActive(false);
            }
        }
    }

    void BuggingMatModification()
    {
        myRend.material.SetFloat("_SinFrequency", 40);
        myRend.material.SetFloat("_AdditiveEmissive", 2.5f);
        myRend.material.SetColor("_Noise1PannerSpeed", new Color(4, 8, 0, 0));
        myRend.material.SetColor("_Noise2PannerSpeed", new Color(-10, 7, 0, 0));
        myRend.material.SetFloat("_RedEmissiveIntensity", Random.Range(1f, 1.2f));
    }

    void SetBuggingState(bool _isBugging)
    {
        if (_isBugging)
        {
            bugging = true;
        }
        else //reset default values
        {
            myRend.material.SetFloat("_SinFrequency", 5);
            myRend.material.SetFloat("_AdditiveEmissive", 0.77f);
            myRend.material.SetColor("_Noise1PannerSpeed", new Color(.1f, .1f, 0, 0));
            myRend.material.SetColor("_Noise2PannerSpeed", new Color(.15f, .15f, 0, 0));
            myRend.material.SetFloat("_RedEmissiveIntensity", 0);
        }
    }

    float RemapValue(float value, float min1, float max1, float min2, float max2)
    {
        return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
    }

    void UpdateEyeLocation()
    {
        Vector3 vectorToPlayer = GetCloserPlayer().position - transform.position;
        vectorToPlayer.y = 0;

        float angleLeft = Vector3.Angle(transform.right, vectorToPlayer);
        float angleForward = Vector3.Angle(transform.forward, vectorToPlayer);
        float angleRight = Vector3.Angle(-transform.right, vectorToPlayer);

        if (angleForward < 100)
        {
            eyesWantedPosition = new Vector3(0, 0, 0);
        }
        else
        {
            float valueToRemap = (angleRight + angleForward) / angleForward;
            eyesWantedPosition = Vector3.Lerp(new Vector3(-maxEyeOffset, 0, 0), new Vector3(maxEyeOffset, 0, 0), RemapValue(valueToRemap, 1.10f, 2f, 0, 1));
        }

        leftEyeTransform.localPosition = Vector3.Lerp(leftEyeTransform.localPosition, eyesWantedPosition, eyeLerpIntensity);
        rightEyeTransform.localPosition = Vector3.Lerp(rightEyeTransform.localPosition, eyesWantedPosition, eyeLerpIntensity);
        topEyeTransform.localPosition = Vector3.Lerp(topEyeTransform.localPosition, eyesWantedPosition, eyeLerpIntensity);
    }

    Transform GetCloserPlayer()
    {
        if(Vector3.Distance(transform.position, player1Transform.position) > Vector3.Distance(transform.position, player2Transform.position))
        {
            return player2Transform;
        }
        else
        {
            return player1Transform;
        }
    }

    public void BreakScreen()
    {
        broken = true;
        myRend.material = brokenScreenMat;
        leftEyeTransform.gameObject.SetActive(false);
        rightEyeTransform.gameObject.SetActive(false);
        leftEyeTransform.gameObject.SetActive(false);
        topEyeTransform.gameObject.SetActive(false);
    }

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default)
    {
        if (!broken)
        {
            BreakScreen();
        }
    }
}
