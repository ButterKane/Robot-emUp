using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenBehavior : MonoBehaviour
{

    Transform player1Transform;
    Transform player2Transform;
    public Transform leftEyeTransform;
    public Transform rightEyeTransform; //0.065
    public enum WhereToLook { Left, Middle, Right };
    WhereToLook whereToLook = WhereToLook.Middle;
    public bool bugging;
    public Renderer myRend;

    // Start is called before the first frame update
    void Start()
    {
        player1Transform = GameManager.playerOne.transform;
        player2Transform = GameManager.playerTwo.transform;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEyeLocation();
        if (bugging)
        {
            myRend.material.SetFloat("_SinFrequency", 40);
            myRend.material.SetFloat("_AdditiveEmissive", 2.5f);
            myRend.material.SetColor("_Noise1PannerSpeed", new Color(4, 8, 0, 0));
            myRend.material.SetColor("_Noise2PannerSpeed", new Color(-10, 7, 0, 0));
            myRend.material.SetFloat("_RedEmissiveIntensity", Random.Range(1f, 1.2f));
        }
        else
        {
            myRend.material.SetFloat("_SinFrequency", 5);
            myRend.material.SetFloat("_AdditiveEmissive", 0.77f);
            myRend.material.SetColor("_Noise1PannerSpeed", new Color(.1f, .1f, 0, 0));
            myRend.material.SetColor("_Noise2PannerSpeed", new Color(.15f, .15f, 0, 0));
            myRend.material.SetFloat("_RedEmissiveIntensity", 0);
        }
    }

    void UpdateEyeLocation()
    {
        float angleLeft = Vector3.Angle(transform.right, transform.position - GetCloserPlayer().position);
        float angleForward = Vector3.Angle(transform.forward, transform.position - GetCloserPlayer().position);
        float angleRight = Vector3.Angle(-transform.right, transform.position - GetCloserPlayer().position);

        if (angleRight < angleForward)
        {
            if (angleRight < angleLeft) //AngleRight
            {
                whereToLook = WhereToLook.Right;
            }
            else //AngleLeft
            {
                whereToLook = WhereToLook.Left;
            }
        }
        else
        {
            if (angleForward < angleLeft) //AngleForward
            {
                whereToLook = WhereToLook.Middle;
            }
            else //AngleLeft
            {
                whereToLook = WhereToLook.Left;
            }
        }

        switch (whereToLook)
        {
            case WhereToLook.Left:
                leftEyeTransform.localPosition = Vector3.Lerp(leftEyeTransform.localPosition, new Vector3(-0.065f, 0, 0), 0.1f);
                rightEyeTransform.localPosition = Vector3.Lerp(rightEyeTransform.localPosition, new Vector3(-0.065f, 0, 0), 0.1f);
                break;
            case WhereToLook.Middle:
                leftEyeTransform.localPosition = Vector3.Lerp(leftEyeTransform.localPosition, Vector3.zero, 0.1f);
                rightEyeTransform.localPosition = Vector3.Lerp(rightEyeTransform.localPosition, Vector3.zero, 0.1f);
                break;
            case WhereToLook.Right:
                leftEyeTransform.localPosition = Vector3.Lerp(leftEyeTransform.localPosition, new Vector3(0.065f, 0, 0), 0.1f);
                rightEyeTransform.localPosition = Vector3.Lerp(rightEyeTransform.localPosition, new Vector3(0.065f, 0, 0), 0.1f);
                break;
        }
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
}
